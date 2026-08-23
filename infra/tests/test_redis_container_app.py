"""Infrastructure contract tests for the Redis Container App migration."""

from pathlib import Path
import unittest


REPO_ROOT = Path(__file__).resolve().parents[2]


def read(relative_path: str) -> str:
    return (REPO_ROOT / relative_path).read_text(encoding="utf-8")


class RedisContainerAppInfrastructureTests(unittest.TestCase):
    def test_redis_container_app_is_private_small_pinned_and_password_protected(self) -> None:
        redis = read("infra/modules/redisContainerApp.bicep")

        self.assertIn("image: 'redis:8.8.2-alpine'", redis)
        self.assertIn("external: false", redis)
        self.assertIn("transport: 'tcp'", redis)
        self.assertIn("targetPort: 6379", redis)
        self.assertIn("--requirepass", redis)
        self.assertIn("--appendonly no", redis)
        self.assertIn("secretRef: 'redis-password'", redis)
        self.assertIn("cpu: json('0.25')", redis)
        self.assertIn("memory: '0.5Gi'", redis)
        self.assertIn("minReplicas: 1", redis)
        self.assertIn("maxReplicas: 1", redis)

    def test_provisioning_replaces_managed_redis_with_container_app_and_exports_host(self) -> None:
        main = read("infra/main.bicep")

        self.assertIn("module redisCache 'modules/redisContainerApp.bicep'", main)
        self.assertNotIn("modules/redisCache.bicep", main)
        self.assertNotIn("output AZURE_REDIS_NAME", main)
        self.assertIn("output REDIS_CACHE_HOST_NAME string", main)

    def test_api_consumes_private_redis_host_and_key_vault_password(self) -> None:
        api = read("infra/api.bicep")
        parameters = read("infra/api.bicepparam")

        self.assertIn("param redisHostName string", api)
        self.assertNotIn("param redisPassword string", api)
        self.assertNotIn("Microsoft.Cache/redisEnterprise", api)
        self.assertIn("name: 'Redis__HostName'", api)
        self.assertIn("name: 'Redis__Password'", api)
        self.assertIn("secretRef: 'redis-password'", api)
        self.assertIn("keyVaultUrl: redisPasswordSecretUrl", api)
        self.assertIn("identity: managedIdentity.id", api)
        self.assertIn("param redisHostName = readEnvironmentVariable('REDIS_CACHE_HOST_NAME', '')", parameters)
        self.assertNotIn("readEnvironmentVariable('REDIS_PASSWORD', '')", parameters)
        self.assertIn("param redisPasswordSecretUrl = readEnvironmentVariable('REDIS_KEY_VAULT_URL', '')", parameters)

    def test_application_builds_redis_connection_string_from_key_vault_password(self) -> None:
        cache_configuration = read(
            "src/server/FakeSurveyGenerator.Application/Infrastructure/Caching/CacheConfigurationExtensions.cs"
        )
        project = read("src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj")

        self.assertIn('builder.Configuration["Redis:HostName"]', cache_configuration)
        self.assertIn('builder.Configuration["Redis:Password"]', cache_configuration)
        self.assertIn('builder.Configuration["ConnectionStrings:cache"]', cache_configuration)
        self.assertIn('password={password}', cache_configuration)
        self.assertIn('builder.AddRedisClientBuilder("cache")', cache_configuration)
        self.assertIn(".WithDistributedCache();", cache_configuration)
        self.assertNotIn("WithAzureAuthentication", cache_configuration)
        self.assertIn("Aspire.StackExchange.Redis.DistributedCaching", project)

    def test_redis_password_is_generated_once_in_key_vault_and_referenced_by_container_apps(self) -> None:
        main = read("infra/main.bicep")
        redis = read("infra/modules/redisContainerApp.bicep")
        api = read("infra/api.bicep")
        identity = read("infra/modules/managedIdentity.bicep")
        pipeline = read(".azdo/pipelines/azure-dev.yml")
        main_parameters = read("infra/main.bicepparam")

        self.assertNotIn("param redisPassword string", main)
        self.assertNotIn("REDIS_PASSWORD", main_parameters)
        self.assertIn("redisPasswordSecretUrl", main)
        self.assertIn("param redisPasswordSecretUrl string", redis)
        self.assertIn("keyVaultUrl: redisPasswordSecretUrl", redis)
        self.assertIn("identity: managedIdentity.id", redis)
        self.assertIn("param redisPasswordSecretUrl string", api)
        self.assertIn("keyVaultUrl: redisPasswordSecretUrl", api)
        self.assertIn("identity: managedIdentity.id", api)
        self.assertIn("keyVaultSecretsUser", identity)
        self.assertIn("4633458b-17de-408a-b874-0445c86b69e6", identity)
        self.assertNotIn("keyVaultSecretsOfficer", identity)
        self.assertNotIn("REDIS_PASSWORD: $(REDIS_PASSWORD)", pipeline)
        self.assertIn("Remove legacy Key Vault Secrets Officer role", pipeline)
        self.assertIn("az role assignment delete --ids", pipeline)

    def test_deployment_script_creates_the_secret_only_when_absent(self) -> None:
        generator = read("infra/modules/redisPassword.bicep")

        self.assertIn("Microsoft.Resources/deploymentScripts", generator)
        self.assertIn("b86a8fe4-44ce-4948-aee5-eccb2c155cd7", generator)
        self.assertIn("az keyvault secret show", generator)
        self.assertIn("az keyvault secret set", generator)
        self.assertIn("openssl rand -hex 32", generator)
        self.assertIn('if [ -z "$secret_id" ]', generator)
        self.assertIn("output secretUrl string", generator)


if __name__ == "__main__":
    unittest.main()
