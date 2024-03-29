param location string = 'South Africa North'
param containerAppEnvironmentName string = 'cae-fake-survey-generator'
param uiContainerAppName string = 'ca-fake-survey-generator-ui'
param apiContainerAppName string = 'ca-fake-survey-generator-api'
param containerRegistryName string = 'acrfakesurveygenerator'
param managedIdentityName string = 'mi-fake-survey-generator'
param redisCacheName string = 'redis-fake-survey-generator'
param sqlServerName string = 'sql-marcel-michau'
param sqlDatabaseName string = 'sqldb-fake-survey-generator'
param applicationInsightsName string = 'appi-fake-survey-generator'
param dnsZoneName string = 'mysecondarydomain.com'
param uiContainerVersion string
param apiContainerVersion string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-08-01-preview' existing = {
  name: containerRegistryName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: managedIdentityName
}

resource redisCache 'Microsoft.Cache/redis@2023-08-01' existing = {
  name: redisCacheName
}

resource sqlServer 'Microsoft.Sql/servers@2023-02-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-02-01-preview' existing = {
  name: sqlDatabaseName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

module uiContainerApp 'modules/containerApp.bicep' = {
  name: 'uiContainerApp'
  params: {
    location: location
    containerAppEnvId: containerAppEnvironment.id
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
        image: '${containerRegistry.properties.loginServer}/fake-survey-generator-ui:${uiContainerVersion}'
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

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: containerAppEnvironmentName
}

module apiContainerApp 'modules/containerApp.bicep' = {
  name: 'apiContainerApp'
  params: {
    location: location
    containerAppEnvId: containerAppEnvironment.id
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
        image: '${containerRegistry.properties.loginServer}/fake-survey-generator-api:${apiContainerVersion}'
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

resource daprSecretStoreComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-02-preview' = {
  name: 'azure-key-vault'
  parent: containerAppEnvironment
  properties: {
    componentType: 'secretstores.azure.keyvault'
    version: 'v1'
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

module frontDoor 'modules/frontDoor.bicep' = {
  name: 'frontDoor'
  params: {
    dnsZoneName: dnsZoneName
    uiOriginHostName: uiContainerApp.outputs.containerAppFqdn
    apiOriginHostName: apiContainerApp.outputs.containerAppFqdn
    cnameRecordName: 'fakesurveygenerator'
    endpointName: 'afd-fake-survey-generator'
  }
}
