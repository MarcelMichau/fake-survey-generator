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

    def test_redis_host_export_uses_container_app_name_for_internal_tcp_routing(self) -> None:
        redis = read("infra/modules/redisContainerApp.bicep")

        self.assertIn("output hostName string = redisContainerApp.name", redis)
        self.assertNotIn("output hostName string = redisContainerApp.properties.configuration.ingress.fqdn", redis)

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

    def test_deployment_scripts_run_in_a_dedicated_aci_subnet_allowed_by_key_vault(self) -> None:
        virtual_network = read("infra/modules/virtualNetwork.bicep")
        key_vault = read("infra/modules/keyVault.bicep")
        deployment_storage = read("infra/modules/deploymentScriptStorage.bicep")
        generator = read("infra/modules/redisPassword.bicep")
        sql = read("infra/modules/sql.bicep")
        main = read("infra/main.bicep")

        self.assertIn("deploymentScriptsSubnetId", virtual_network)
        self.assertIn("10.0.0.32/27", virtual_network)
        self.assertIn("Microsoft.ContainerInstance/containerGroups", virtual_network)
        self.assertIn("Microsoft.KeyVault", virtual_network)
        self.assertIn("Microsoft.Storage", virtual_network)
        self.assertIn("deploymentScriptStorage", main)
        self.assertIn("deploymentScriptStorageAccountName", main)
        self.assertIn("deploymentScriptStorageAccountName", generator)
        self.assertIn("storageAccountSettings", generator)
        self.assertIn("deploymentScriptStorageAccountName", sql)
        self.assertIn("storageAccountSettings", sql)
        self.assertIn("storageFileDataPrivilegedContributor", generator)
        self.assertIn("storageFileDataPrivilegedContributorRoleAssignment", generator)
        self.assertIn("storageFileDataPrivilegedContributorRoleAssignment", deployment_storage)
        self.assertIn("deploymentScriptIdentityPrincipalId", deployment_storage)
        self.assertIn("deploymentScriptIdentityPrincipalId: managedIdentity.outputs.principalId", main)
        self.assertIn("param subnetResourceIds array", key_vault)
        self.assertIn("for subnetResourceId in subnetResourceIds", key_vault)
        self.assertIn("param deploymentScriptSubnetResourceId string", generator)
        self.assertIn("containerSettings", generator)
        self.assertIn("id: deploymentScriptSubnetResourceId", generator)
        self.assertIn("param deploymentScriptSubnetResourceId string", sql)
        self.assertIn("sqlDeploymentScriptsVirtualNetworkRules", sql)
        self.assertIn("virtualNetworkSubnetId: deploymentScriptSubnetResourceId", sql)
        self.assertIn(
            "dependsOn: [\n    sqlServer::sqlDatabase\n    sqlServer::sqlDeploymentScriptsVirtualNetworkRules\n  ]",
            sql,
        )
        self.assertIn("containerSettings", sql)
        self.assertIn("id: deploymentScriptSubnetResourceId", sql)
        self.assertIn("deploymentScriptSubnetResourceId: virtualNetwork.outputs.deploymentScriptsSubnetId", main)
        self.assertIn("subnetResourceIds: [", main)

    def test_sql_deployment_script_uses_an_access_token_without_the_sqlserver_module(self) -> None:
        sql_script = read("infra/modules/sql-deployment-script.ps1")

        self.assertNotIn("Install-Module -Name SqlServer", sql_script)
        self.assertNotIn("Import-Module SqlServer", sql_script)
        self.assertNotIn("Invoke-Sqlcmd", sql_script)
        self.assertIn("Get-AzAccessToken -ResourceUrl 'https://database.windows.net/'", sql_script)
        self.assertIn("[System.Data.SqlClient.SqlConnection]", sql_script)
        self.assertIn("AccessToken", sql_script)
        self.assertIn("ExecuteNonQuery", sql_script)

    def test_sql_deployment_script_executes_dynamic_sql_from_a_variable(self) -> None:
        sql_script = read("infra/modules/sql-deployment-script.ps1")

        self.assertIn("DECLARE @sql NVARCHAR(MAX);", sql_script)
        self.assertIn("SET @sql = N'CREATE USER '", sql_script)
        self.assertIn("SET @sql = N'ALTER ROLE [db_owner] ADD MEMBER '", sql_script)
        self.assertIn("EXEC sys.sp_executesql @sql;", sql_script)
        self.assertNotIn("EXEC(N'CREATE USER ' +", sql_script)
        self.assertNotIn("EXEC(N'ALTER ROLE [db_owner] ADD MEMBER ' +", sql_script)


if __name__ == "__main__":
    unittest.main()
