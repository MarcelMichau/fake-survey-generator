# Azure DevOps CI/CD

The pipeline in `pipelines/azure-dev.yml` validates PRs **targeting `main`**. Its path filters include server/UI code, infrastructure, API contracts, Dapr configuration, and root build/deployment inputs.

## Main-branch workflow

Three jobs can run concurrently (subject to the organization's parallel-job capacity):

1. Backend restore/build/tests and migration-script generation.
2. Frontend contract check, build, and browser tests.
3. Candidate container packaging and publication.

The `Package` job is main-only and uses the existing Azure service connection. It computes the application SemVer and a separate release identity, for example `6.0.247-build7122-attempt1`. The build ID and job attempt prevent candidate-tag collisions on retries; the application assembly and `VITE_APP_VERSION` retain the original SemVer.

`publish-image.sh` refreshes the existing azd environment, runs `azd publish api`, resolves the published tag to a SHA-256 digest, and emits that digest as a job output. Failed validation may leave an unused candidate in ACR, but cannot deploy it. Registry retention/cleanup is a separate operational concern; this pipeline does not delete images.

After both validation stages (including candidate publication) succeed, infrastructure is provisioned and the SQL migration artifact is applied. Only then does `deploy-image.sh` call `azd deploy api --from-package <registry/repository@sha256:...>`. This deploys the exact candidate without building or pushing again. `API_VERSION` supplies the unique revision suffix to `infra/api.bicepparam`.

Each hosted job initializes and refreshes its own azd environment. `.azure` is excluded from Docker context and is never published as an artifact. The TypeSafe API key is still required when evaluating the infrastructure parameters during refresh.

### Bootstrap requirement

Parallel publication requires the dev environment, its deployment outputs, and ACR to exist already. The service connection needs image push/read access. If the environment has been deleted or a change replaces the registry, bootstrap/migrate the infrastructure through an explicitly approved infrastructure operation before using this workflow. Publishing deliberately fails instead of provisioning live resources before validation. The deploy script also rejects a candidate from a different environment registry.

## Validation details

- `TF_BUILD` enables `ContinuousIntegrationBuild` and locked NuGet restore for every server project, including the API-contract generator. The Docker build sets the CI property explicitly.
- Backend tests still run in Debug with `--coverage --report-trx`. MTP produces individual reports plus a merged report. CI publishes **only the merged report**, or individual reports if post-processing did not produce a merged report. Standard TRX coverage attachments are preserved; there is no additional coverage publisher.
- Missing test reports and failed tests fail validation. Results are published on both success and failure.
- EF is installed from the version in `.config/dotnet-tools.json`; unrelated tools are not restored. Migration SQL is generated with `--no-build`, including on PRs.
- Browser installation uses Chromium headless shell in CI. Acceptance tests retain full Chromium installation outside Azure Pipelines for local headed debugging.
- Docker pruning, disk snapshots, development-certificate trust and Dapr initialization remain in place. Dapr CLI/runtime and azd versions are pinned in pipeline variables; update those pins deliberately.
- Linux jobs use `ubuntu-24.04`. The Node major is explicitly selected in both validation jobs.
- No persistent dependency caches or conditional infrastructure provisioning are introduced.

## Regression checks

```bash
python3 .azdo/tests/test_pipeline.py
```

These standard-library tests check workflow invariants and execute the shell scripts against fake `az`/`azd` commands. They do not contact Azure. On Windows, use Git Bash rather than WSL Bash, for example from Git Bash:

```bash
BASH_EXECUTABLE="$(cygpath -w "$(command -v bash)")" python .azdo/tests/test_pipeline.py
```

## First pipeline-run checklist

- PR: candidate publication and all deployment stages are skipped; backend/UI tests and migration generation pass.
- Main: packaging overlaps validation; the published image uses the build/attempt-qualified tag.
- Backend results show 188 tests for the current test suite, not 376; coverage remains visible (run 7122 reported 80.80%). Test totals will naturally change as tests are added.
- Deployment logs show the `--from-package` digest and no Docker packaging/push step.
- The deployed revision references that digest and the build/attempt-qualified suffix; the displayed application version remains SemVer.
- Compare total duration with run 7122 (9m 42s), accounting for hosted-agent queues and image-download variability.
