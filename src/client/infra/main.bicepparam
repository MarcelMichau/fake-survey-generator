using './main.bicep'

param containerAppName = readEnvironmentVariable('SERVICE_UI_NAME', '')
param containerAppEnvironmentId = readEnvironmentVariable('AZURE_CONTAINER_APPS_ENVIRONMENT_ID', '')
param containerRegistryUrl = readEnvironmentVariable('AZURE_CONTAINER_REGISTRY_ENDPOINT', '')
param imageName = readEnvironmentVariable('SERVICE_UI_IMAGE_NAME', '')
param managedIdentityName = readEnvironmentVariable('SERVICE_UI_IDENTITY_NAME', '')
