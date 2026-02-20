using './api.bicep'

param containerAppName = readEnvironmentVariable('SERVICE_API_NAME', '')
param containerAppEnvironmentId = readEnvironmentVariable('AZURE_CONTAINER_APPS_ENVIRONMENT_ID', '')
param containerRegistryUrl = readEnvironmentVariable('AZURE_CONTAINER_REGISTRY_ENDPOINT', '')
param imageName = readEnvironmentVariable('SERVICE_API_IMAGE_NAME', '')
param managedIdentityName = readEnvironmentVariable('SERVICE_API_IDENTITY_NAME', '')
param sqlServerName = readEnvironmentVariable('SQL_SERVER_NAME', '')
param sqlDatabaseName = readEnvironmentVariable('SQL_DATABASE_NAME', '')
param redisCacheName = readEnvironmentVariable('AZURE_REDIS_NAME', '')
param applicationInsightsName = readEnvironmentVariable('AZURE_APPLICATION_INSIGHTS_NAME', '')
param version = readEnvironmentVariable('API_VERSION', 'latest')
param activeLabel = readEnvironmentVariable('API_ACTIVE_LABEL', 'blue')
param promotePreview = toLower(readEnvironmentVariable('API_PROMOTE_PREVIEW', 'false')) == 'true'
