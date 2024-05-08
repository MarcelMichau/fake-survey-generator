param tags object
param location string
param containerAppEnvironmentName string
param uiContainerAppName string
param apiContainerAppName string
param containerRegistryName string
param managedIdentityName string
param redisCacheName string
param sqlServerName string
param sqlDatabaseName string
param logAnalyticsName string
param applicationInsightsName string
param virtualNetworkSubnetId string

param apiContainerAppExists bool
param uiContainerAppExists bool

param apiContainerImage string = ''
param uiContainerImage string = ''

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: containerRegistryName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' existing = {
  name: managedIdentityName
}

resource redisCache 'Microsoft.Cache/redis@2023-08-01' existing = {
  name: redisCacheName
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' existing = {
  name: sqlDatabaseName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource existingUiContainerApp 'Microsoft.App/containerApps@2024-03-01' existing = if (uiContainerAppExists) {
  name: uiContainerAppName
}

module containerAppEnvironment 'modules/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironment'
  params: {
    location: location
    tags: tags
    logAnalyticsName: logAnalyticsName
    containerAppEnvName: containerAppEnvironmentName
    subnetResourceId: virtualNetworkSubnetId
  }
}

module uiContainerApp 'modules/containerApp.bicep' = {
  name: 'uiContainerApp'
  params: {
    location: location
    tags: union(tags, { 'azd-service-name': 'ui' })
    containerAppEnvId: containerAppEnvironment.outputs.containerAppEnvironmentId
    containerAppName: uiContainerAppName
    containerRegistryUrl: containerRegistry.properties.loginServer
    identityType: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
    containerRegistryIdentity: managedIdentity.id
    containers: [
      {
        name: 'fake-survey-generator-ui'
        image: !empty(uiContainerImage)
          ? uiContainerImage
          : uiContainerAppExists
              ? existingUiContainerApp.properties.template.containers[0].image
              : 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
      }
    ]
  }
}

var apiEnvironmentVariables = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'ConnectionStrings__database'
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Managed Identity;User Id=${managedIdentity.properties.clientId};'
  }
  {
    name: 'ConnectionStrings__cache'
    value: '${redisCache.properties.hostName},defaultDatabase=0,ssl=true,password=${redisCache.listKeys().primaryKey},abortConnect=false'
  }
  {
    name: 'IDENTITY_PROVIDER_URL'
    value: 'https://marcelmichau.eu.auth0.com/'
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: applicationInsights.properties.ConnectionString
  }
]

resource existingApiContainerApp 'Microsoft.App/containerApps@2024-03-01' existing = if (apiContainerAppExists) {
  name: apiContainerAppName
}

module apiContainerApp 'modules/containerApp.bicep' = {
  name: 'apiContainerApp'
  params: {
    location: location
    tags: union(tags, { 'azd-service-name': 'api' })
    containerAppEnvId: containerAppEnvironment.outputs.containerAppEnvironmentId
    containerAppName: apiContainerAppName
    containerRegistryUrl: containerRegistry.properties.loginServer
    identityType: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
    containerRegistryIdentity: managedIdentity.id
    containers: [
      {
        name: 'fake-survey-generator-api'
        image: !empty(apiContainerImage)
          ? apiContainerImage
          : apiContainerAppExists
              ? existingApiContainerApp.properties.template.containers[0].image
              : 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        env: apiEnvironmentVariables
      }
    ]
    targetPort: 8080
    daprConfig: {
      enabled: true
      appId: 'fake-survey-generator-api'
      appProtocol: 'http'
      appPort: 8080
    }
  }
}

module daprSecretStoreComponent 'modules/daprComponent.bicep' = {
  name: 'daprSecretStoreComponent'
  params: {
    componentName: 'secrets'
    containerAppEnvironmentName: containerAppEnvironment.outputs.containerAppEnvironmentName
    componentType: 'secretstores.azure.keyvault'
    metadata: [
      {
        name: 'vaultName'
        value: 'kv-fake-survey-generator'
      }
      {
        name: 'azureTenantId'
        value: subscription().tenantId
      }
      {
        name: 'azureClientId'
        value: managedIdentity.properties.clientId
      }
    ]
    scopes: [
      'fake-survey-generator-api'
    ]
  }
}

output apiContainerAppFqdn string = apiContainerApp.outputs.containerAppFqdn
output apiContainerAppIdentityName string = managedIdentity.name
output apiContainerAppIdentityPrincipalId string = managedIdentity.properties.principalId
output apiContainerAppName string = apiContainerApp.outputs.containerAppName
output apiContainerAppUri string = apiContainerApp.outputs.containerAppUri
output apiContainerImageName string = apiContainerImage

output uiContainerAppFqdn string = uiContainerApp.outputs.containerAppFqdn
output uiContainerAppIdentityName string = managedIdentity.name
output uiContainerAppIdentityPrincipalId string = managedIdentity.properties.principalId
output uiContainerAppName string = uiContainerApp.outputs.containerAppName
output uiContainerAppUri string = uiContainerApp.outputs.containerAppUri
output uiContainerImageName string = uiContainerImage
