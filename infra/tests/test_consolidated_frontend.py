"""Infrastructure and container contract tests for the consolidated application."""

import json
from pathlib import Path
import unittest


REPO_ROOT = Path(__file__).resolve().parents[2]


def read(relative_path: str) -> str:
    return (REPO_ROOT / relative_path).read_text(encoding="utf-8")


class ConsolidatedFrontendTests(unittest.TestCase):
    def test_api_dockerfile_builds_frontend_and_copies_dist_to_wwwroot(self) -> None:
        dockerfile = read("src/server/FakeSurveyGenerator.Api/Dockerfile")

        self.assertIn("FROM node:", dockerfile)
        self.assertIn("src/client/ui/package.json", dockerfile)
        self.assertIn("npm run build", dockerfile)
        self.assertIn("ARG VITE_APP_VERSION", dockerfile)
        self.assertIn("COPY --from=frontend-build", dockerfile)
        self.assertIn("/wwwroot", dockerfile)

    def test_production_azd_configuration_deploys_only_the_api_service(self) -> None:
        azure_yaml = read("azure.yaml")
        pipeline = read(".azdo/pipelines/azure-dev.yml")

        self.assertIn("  api:", azure_yaml)
        self.assertNotIn("  ui:", azure_yaml)
        self.assertIn('image: fake-survey-generator/fake-survey-generator-api', azure_yaml)
        self.assertNotIn('fake-survey-generator/fake-survey-generator-ui', azure_yaml)
        self.assertIn('"VITE_APP_VERSION"', azure_yaml)
        self.assertNotIn("UI_VERSION", pipeline)
        self.assertNotIn("uiSemVerVersionTag", pipeline)
        self.assertIn("VITE_APP_VERSION: $(apiSemVerVersionTag)", pipeline)
        self.assertNotIn("Deploy Application Without Custom Domain", pipeline)
        self.assertNotIn("Configure Container App Domain Validation", pipeline)
        self.assertNotIn("Bind Container App Custom Domain", pipeline)
        self.assertNotIn("properties.customDomainVerificationId", pipeline)
        self.assertNotIn("asuid.fakesurveygenerator", pipeline)
        self.assertNotIn("existingVerificationId", pipeline)
        self.assertNotIn("if [ \"$existingVerificationId\" != \"$verificationId\" ]", pipeline)
        self.assertIn("Verify Container App Custom Domain", pipeline)
        self.assertIn("properties.configuration.ingress.customDomains", pipeline)
        self.assertEqual(pipeline.count("azd deploy --no-prompt"), 1)
        self.assertIn("CUSTOM_DOMAIN_NAME: fakesurveygenerator.mysecondarydomain.com", pipeline)

    def test_api_version_tracks_frontend_changes(self) -> None:
        version = json.loads(read("src/server/version.json"))

        self.assertIn(":/src/client/ui", version["pathFilters"])

    def test_infrastructure_deploys_one_public_app_with_direct_custom_domain(self) -> None:
        main = read("infra/main.bicep")
        dns_zone = read("infra/modules/dnsZone.bicep")
        api = read("infra/api.bicep")
        api_params = read("infra/api.bicepparam")
        azure_yaml = read("azure.yaml")
        pipeline = read(".azdo/pipelines/azure-dev.yml")
        environment = read("infra/modules/containerAppEnvironment.bicep")

        self.assertNotIn("modules/ui.bicep", main)
        self.assertFalse((REPO_ROOT / "infra/ui.bicep").exists())
        self.assertFalse((REPO_ROOT / "infra/ui.bicepparam").exists())
        self.assertFalse((REPO_ROOT / "src/client/ui/Dockerfile").exists())
        self.assertNotIn("SERVICE_UI_NAME", main)
        self.assertNotIn("SERVICE_UI_IDENTITY_NAME", main)
        self.assertNotIn("frontDoor", main)
        self.assertFalse((REPO_ROOT / "infra/modules/frontDoor.bicep").exists())
        self.assertIn("cnameTargetHostName:", main)
        self.assertIn("CNAMERecord:", dns_zone)
        self.assertIn("cnameTargetHostName", dns_zone)
        self.assertIn("customDomains:", api)
        self.assertIn("bindingType: 'Auto'", api)
        self.assertNotIn("Configure Container App Domain Validation", pipeline)
        self.assertNotIn("properties.customDomainVerificationId", pipeline)
        self.assertNotIn("asuid.fakesurveygenerator", pipeline)
        self.assertIn("managedCertificates@2026-01-01", api)
        self.assertNotIn("validationTxtRecord", api)
        self.assertIn("dependsOn: [containerApp]", api)
        self.assertIn("customDomainVerificationId", dns_zone)
        self.assertIn("TXTRecords", dns_zone)
        self.assertIn("asuid.", dns_zone)
        self.assertIn("customDomainVerificationId", main)
        self.assertIn("customDomainVerificationId", environment)
        self.assertIn("param customDomainName", api_params)
        self.assertIn("apiVersion: 2026-01-01", azure_yaml)
        self.assertIn("output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME", main)
        self.assertIn("output DNS_ZONE_NAME", main)
        self.assertIn("output CUSTOM_DOMAIN_NAME", main)
        self.assertNotIn("ca-fake-survey-generator-ui", environment)
        self.assertIn("prefix: '/'", environment)


if __name__ == "__main__":
    unittest.main()
