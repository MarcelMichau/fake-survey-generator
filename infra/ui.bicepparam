using './ui.bicep'

param containerAppName = readEnvironmentVariable('SERVICE_UI_NAME', '')
param containerAppEnvironmentId = readEnvironmentVariable('AZURE_CONTAINER_APPS_ENVIRONMENT_ID', '')
param containerRegistryUrl = readEnvironmentVariable('AZURE_CONTAINER_REGISTRY_ENDPOINT', '')
param imageName = readEnvironmentVariable('SERVICE_UI_IMAGE_NAME', '')
param managedIdentityName = readEnvironmentVariable('SERVICE_UI_IDENTITY_NAME', '')
param version = readEnvironmentVariable('UI_VERSION', 'latest')
param activeLabel = readEnvironmentVariable('UI_ACTIVE_LABEL', 'blue')
param promotePreview = toLower(readEnvironmentVariable('UI_PROMOTE_PREVIEW', 'false')) == 'true'
param productionRevisionName = readEnvironmentVariable('UI_PRODUCTION_REVISION_NAME', '')
