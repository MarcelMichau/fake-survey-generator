"""CI regression checks; standard library only, no Azure access or container builds.

Run from the repository root: python3 .azdo/tests/test_pipeline.py
"""
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
PIPELINE = (ROOT / ".azdo/pipelines/azure-dev.yml").read_text(encoding="utf-8")
SCRIPTS = ROOT / ".azdo/scripts"
BASH = os.environ.get("BASH_EXECUTABLE") or shutil.which("bash")
DIGEST = "sha256:" + "a" * 64
REGISTRY = "testregistry.azurecr.io"
RELEASE = "6.0.247-build7122-attempt1"
IMAGE = f"{REGISTRY}/fake-survey-generator/fake-survey-generator-api"


class PipelineStructureTests(unittest.TestCase):
    def test_pr_targets_main_and_both_triggers_cover_build_inputs(self):
        pr = PIPELINE.split("\npr:\n", 1)[1].split("\nvariables:", 1)[0]
        self.assertEqual(re.findall(r"^      - (.+)$", pr.split("  paths:", 1)[0], re.M), ["main"])
        for trigger in [PIPELINE.split("\npr:", 1)[0], pr]:
            for path in ["azure.yaml", "NuGet.config", "FakeSurveyGenerator.slnx",
                         ".config/dotnet-tools.json", ".dockerignore", "dapr/*"]:
                self.assertIn(f"      - {path}\n", trigger)

    def test_packaging_is_main_only_and_independent_of_validation(self):
        package = PIPELINE.split("      - job: Package\n", 1)[1].split("      - job: Build\n", 1)[0]
        self.assertIn("condition: and(succeeded(), eq(variables.isMain, true))", package)
        self.assertNotIn("dependsOn:", package)
        self.assertIn("-build$(Build.BuildId)-attempt$(System.JobAttempt)", package)
        self.assertIn("VITE_APP_VERSION: $(NBGV_SemVer1)", package)
        self.assertIn("API_VERSION: $(SetReleaseVersion.ReleaseVersion)", package)
        deploy = PIPELINE.split("  - stage: App_Deployment\n", 1)[1]
        for dependency in ["BuildTestApi", "BuildTestUI", "Infrastructure_Database_Deployment"]:
            self.assertIn(f"      - {dependency}\n", deploy)
        self.assertIn("condition: and(succeeded(), eq(variables.isMain, true))", deploy)
        self.assertIn("Package.outputs['PublishImage.ImageDigest']", deploy)
        self.assertIn("Package.outputs['SetReleaseVersion.ReleaseVersion']", deploy)
        self.assertNotIn("UseDotNet@", deploy)

    def test_coverage_and_single_report_publication_are_preserved(self):
        self.assertIn("publishTestResults: false", PIPELINE)
        self.assertIn("--coverage --report-trx", PIPELINE)
        self.assertIn('testResultsFiles: "$(BackendTestResultsPattern)"', PIPELINE)
        self.assertIn("publishRunAttachments: true", PIPELINE)
        self.assertIn("failTaskOnMissingResultsFile: true", PIPELINE)
        self.assertEqual(PIPELINE.count("task: PublishCodeCoverageResults@2"), 1)
        coverage = PIPELINE.split("- task: PublishCodeCoverageResults@2", 1)[1].split("- script:", 1)[0]
        self.assertIn("condition: succeededOrFailed()", coverage)
        self.assertIn('summaryFileLocation: "$(Agent.TempDirectory)/backend-tests/$(BackendCoveragePattern)"', coverage)
        self.assertIn('pathToSources: "$(Build.SourcesDirectory)"', coverage)
        self.assertIn("failIfCoverageEmpty: true", coverage)
        self.assertIn("--only-shell chromium", PIPELINE)

    def test_locked_restore_and_no_redundant_build_or_tools(self):
        self.assertIn("dotnet restore FakeSurveyGenerator.slnx --locked-mode", PIPELINE)
        self.assertIn('--no-restore --configuration Debug', PIPELINE)
        self.assertNotIn("dotnet tool restore", PIPELINE)
        self.assertIn("--no-build --configuration Debug --output DbMigrationScript.sql", PIPELINE)
        props = (ROOT / "src/server/Directory.Build.props").read_text(encoding="utf-8-sig")
        self.assertIn("$(WarningsAsErrors);NU1900", props)
        self.assertIn("<RestoreLockedMode Condition=", props)
        dockerfile = (ROOT / "src/server/FakeSurveyGenerator.Api/Dockerfile").read_text()
        self.assertNotIn("RUN dotnet build", dockerfile)
        self.assertIn('dotnet publish "./FakeSurveyGenerator.Api.csproj" --no-restore', dockerfile)
        self.assertLess(dockerfile.index("RUN npm ci"), dockerfile.index("ARG VITE_APP_VERSION"))


@unittest.skipUnless(BASH, "Bash required for script regression tests")
class ScriptTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.directory = Path(self.temp.name)
        self.log = self.directory / "commands.log"
        self.env = dict(os.environ,
                        AZURE_ENV_NAME="dev", AZURE_SUBSCRIPTION_ID="test-subscription",
                        AZURE_LOCATION="South Africa North", TYPE_SAFE_API_KEY="test-only",
                        API_VERSION=RELEASE, VITE_APP_VERSION="6.0.247",
                        IMAGE_DIGEST=f"{IMAGE}@{DIGEST}", MOCK_REGISTRY=REGISTRY,
                        MOCK_IMAGE=f"{IMAGE}:{RELEASE}", MOCK_DIGEST=DIGEST,
                        COMMAND_LOG=self.log.as_posix(),
                        TEST_RESULTS_DIRECTORY=(self.directory / "results").as_posix())
        self.env["PATH"] = str(self.directory) + os.pathsep + os.environ["PATH"]
        self.mock("azd", '''
printf '%s\n' "azd $*" >> "$COMMAND_LOG"
case "$*" in
  'env refresh '*) exit "${MOCK_REFRESH_EXIT:-0}" ;;
  'env get-value AZURE_CONTAINER_REGISTRY_ENDPOINT') echo "$MOCK_REGISTRY" ;;
  'env get-value SERVICE_API_IMAGE_NAME') echo "$MOCK_IMAGE" ;;
  'publish '*) exit "${MOCK_PUBLISH_EXIT:-0}" ;;
esac
''')
        self.mock("az", '''
printf '%s\n' "az $*" >> "$COMMAND_LOG"
echo "$MOCK_DIGEST"
''')

    def mock(self, name, body):
        executable = self.directory / name
        executable.write_text("#!/usr/bin/env bash\nset -euo pipefail\n" + body, newline="\n")
        executable.chmod(0o755)

    def run_script(self, name):
        return subprocess.run([BASH, (SCRIPTS / name).as_posix()], env=self.env,
                              cwd=ROOT, capture_output=True, text=True)

    def commands(self):
        return self.log.read_text() if self.log.exists() else ""

    def test_publish_outputs_digest_without_deploying(self):
        result = self.run_script("publish-image.sh")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn(f"variable=ImageDigest;isOutput=true]{IMAGE}@{DIGEST}", result.stdout)
        self.assertIn("azd publish api --no-prompt", self.commands())
        self.assertNotIn("azd deploy", self.commands())
        self.assertNotIn("azd provision", self.commands())

    def test_failed_refresh_cannot_publish_or_deploy(self):
        self.env["MOCK_REFRESH_EXIT"] = "1"
        for script in ["publish-image.sh", "deploy-image.sh"]:
            result = self.run_script(script)
            self.assertNotEqual(result.returncode, 0)
        self.assertNotIn("azd publish", self.commands())
        self.assertNotIn("azd deploy", self.commands())

    def test_failed_publish_does_not_emit_a_deployable_output(self):
        self.env["MOCK_PUBLISH_EXIT"] = "1"
        result = self.run_script("publish-image.sh")
        self.assertNotEqual(result.returncode, 0)
        self.assertNotIn("variable=ImageDigest", result.stdout)
        self.assertNotIn("az acr", self.commands())

    def test_invalid_digest_or_unexpected_tag_cannot_be_published_as_output(self):
        for key, value in [("MOCK_DIGEST", "null"), ("MOCK_IMAGE", f"{IMAGE}:latest")]:
            with self.subTest(key=key):
                original = self.env[key]
                self.env[key] = value
                result = self.run_script("publish-image.sh")
                self.assertNotEqual(result.returncode, 0)
                self.assertNotIn("variable=ImageDigest", result.stdout)
                self.env[key] = original

    def test_deploy_uses_exact_digest_without_republishing(self):
        result = self.run_script("deploy-image.sh")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn(f"azd deploy api --from-package {IMAGE}@{DIGEST} --no-prompt", self.commands())
        self.assertNotIn("azd publish", self.commands())

    def test_mutable_tag_or_missing_output_cannot_deploy(self):
        for value in [f"{IMAGE}:latest", "", "$(imageDigest)"]:
            self.env["IMAGE_DIGEST"] = value
            self.assertNotEqual(self.run_script("deploy-image.sh").returncode, 0)
        self.assertNotIn("azd deploy", self.commands())

    def test_different_environment_registry_cannot_deploy(self):
        self.env["MOCK_REGISTRY"] = "anotherregistry.azurecr.io"
        self.assertNotEqual(self.run_script("deploy-image.sh").returncode, 0)
        self.assertNotIn("azd deploy", self.commands())

    def test_results_prefer_merged_and_fall_back_on_failure(self):
        result = self.run_script("select-test-results.sh")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("BackendTestResultsPattern]**/*.trx", result.stdout)
        merged = self.directory / "results/merged"
        merged.mkdir(parents=True)
        (merged / "merged.trx").write_text("<TestRun />")
        result = self.run_script("select-test-results.sh")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("BackendTestResultsPattern]merged/*.trx", result.stdout)

    def test_coverage_prefers_merged_without_duplicate_attachment_copies(self):
        results = self.directory / "results"
        merged = results / "merged"
        attachments = results / "test-run/In/agent"
        merged.mkdir(parents=True)
        attachments.mkdir(parents=True)
        (results / "project.coverage").write_bytes(b"coverage")
        (attachments / "project.coverage").write_bytes(b"coverage")
        (merged / "merged.coverage").write_bytes(b"merged coverage")
        result = self.run_script("select-test-results.sh")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("BackendCoveragePattern]merged/*.coverage", result.stdout)
        self.assertNotIn("BackendCoveragePattern]**/*.coverage", result.stdout)

    def test_coverage_fallback_is_independent_of_trx_post_processing(self):
        merged = self.directory / "results/merged"
        merged.mkdir(parents=True)
        (merged / "merged.trx").write_text("<TestRun />")
        (self.directory / "results/project.coverage").write_bytes(b"coverage")
        result = self.run_script("select-test-results.sh")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("BackendTestResultsPattern]merged/*.trx", result.stdout)
        self.assertIn("BackendCoveragePattern]*.coverage", result.stdout)
        self.assertNotIn("BackendCoveragePattern]**/*.coverage", result.stdout)


if __name__ == "__main__":
    unittest.main(verbosity=2)
