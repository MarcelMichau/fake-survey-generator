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
- Backend tests still run in Debug with `--coverage --report-trx`. MTP produces individual reports plus a merged report. CI publishes **only the merged TRX**, or individual reports if post-processing did not produce a merged report. Standard TRX coverage attachments are preserved. `PublishCodeCoverageResults@2` explicitly publishes the merged `.coverage` file to populate the pipeline coverage UI; it falls back to top-level per-project `.coverage` files if coverage post-processing did not complete. Nested attachment copies are excluded. Test reports and coverage are selected independently, and no tests are rerun.
- Missing test reports and failed tests fail validation. Results are published on both success and failure.
- EF is installed from the version in `.config/dotnet-tools.json`; unrelated tools are not restored. Migration SQL is generated with `--no-build`, including on PRs.
- Browser installation uses Chromium headless shell in CI. Acceptance tests retain full Chromium installation outside Azure Pipelines for local headed debugging.
- Docker pruning, disk snapshots, development-certificate trust and Dapr initialization remain in place. Dapr CLI/runtime and azd versions are pinned in pipeline variables; update those pins deliberately.
- Linux jobs use `ubuntu-24.04`. The Node major is explicitly selected in both validation jobs.
- Every azd service job installs the SDK pinned in `global.json`, including infrastructure provisioning and prebuilt-image deployment. azd initializes C# user secrets on the host even when no host compilation is needed.
- No persistent dependency caches or conditional infrastructure provisioning are introduced.

## Regression checks

```bash
python3 .azdo/tests/test_pipeline.py
```

These standard-library tests check workflow invariants and execute the shell scripts against fake `az`/`azd` commands. They do not contact Azure. On Windows, use Git Bash rather than WSL Bash, for example from Git Bash:

```bash
BASH_EXECUTABLE="$(cygpath -w "$(command -v bash)")" python .azdo/tests/test_pipeline.py
```

## Hosted validation findings

PR validation run [7123](https://dev.azure.com/marcelmichau/Personal/_build/results?buildId=7123&view=results) succeeded with 188 backend tests and 83 UI tests. Backend validation took 248s (versus 272s in run 7122); UI validation took 104s (versus 164s). Playwright UI installation fell from 84s to 27s. These are single-run comparisons, not guaranteed savings.

Run 7123 generated merged coverage, but Azure's coverage API returned an empty `coverageData` array. The merged TRX's coverage references were not sufficient to populate the coverage UI. Explicit coverage publication was added as a follow-up; the next hosted run must verify that the UI is restored. Candidate packaging and application deployment were skipped, as expected for a PR.

Main run [7126](https://dev.azure.com/marcelmichau/Personal/_build/results?buildId=7126&view=results) successfully published the candidate image and digest but failed in infrastructure provisioning: azd invoked `dotnet user-secrets` during C# service initialization, and the hosted agent lacked the SDK pinned in `global.json`. SDK setup was restored for both provisioning and prebuilt application deployment; those jobs must not rely on the hosted image's preinstalled SDKs. The next main run must verify provisioning and the digest-based deployment handoff.

## First pipeline-run checklist

- PR: candidate publication and all deployment stages are skipped; backend/UI tests and migration generation pass.
- Main: packaging overlaps validation; the published image uses the build/attempt-qualified tag.
- Backend results show 188 tests for the current test suite, not 376; coverage remains visible (run 7122 reported 80.80%). Test totals will naturally change as tests are added.
- Deployment logs show the `--from-package` digest and no Docker packaging/push step.
- The deployed revision references that digest and the build/attempt-qualified suffix; the displayed application version remains SemVer.
- Compare total duration with run 7122 (9m 42s), accounting for hosted-agent queues and image-download variability.
