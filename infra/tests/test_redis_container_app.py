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

    def test_api_consumes_the_private_redis_connection_string_as_a_secret(self) -> None:
        api = read("infra/api.bicep")
        parameters = read("infra/api.bicepparam")

        self.assertIn("param redisHostName string", api)
        self.assertIn("param redisPassword string", api)
        self.assertNotIn("Microsoft.Cache/redisEnterprise", api)
        self.assertIn("name: 'cache-connection-string'", api)
        self.assertIn("secretRef: 'cache-connection-string'", api)
        self.assertIn("ssl=false", api)
        self.assertIn("param redisHostName = readEnvironmentVariable('REDIS_CACHE_HOST_NAME', '')", parameters)
        self.assertIn("param redisPassword = readEnvironmentVariable('REDIS_PASSWORD', '')", parameters)

    def test_application_uses_password_authenticated_redis_client(self) -> None:
        cache_configuration = read(
            "src/server/FakeSurveyGenerator.Application/Infrastructure/Caching/CacheConfigurationExtensions.cs"
        )
        project = read("src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj")

        self.assertIn('builder.AddRedisClientBuilder("cache")', cache_configuration)
        self.assertIn(".WithDistributedCache();", cache_configuration)
        self.assertNotIn("WithAzureAuthentication", cache_configuration)
        self.assertIn("Aspire.StackExchange.Redis.DistributedCaching", project)

    def test_pipeline_supplies_the_redis_password_to_provisioning_and_api_deployment(self) -> None:
        pipeline = read(".azdo/pipelines/azure-dev.yml")

        self.assertGreaterEqual(pipeline.count("REDIS_PASSWORD: $(REDIS_PASSWORD)"), 2)


if __name__ == "__main__":
    unittest.main()
