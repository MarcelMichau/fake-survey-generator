param location string = 'South Africa North'
param containerAppEnvironmentName string = 'cae-fake-survey-generator'
param uiContainerAppName string = 'ca-fake-survey-generator-ui'
param apiContainerAppName string = 'ca-fake-survey-generator-api'
param containerRegistryName string = 'acrfakesurveygenerator'
param managedIdentityName string = 'mi-fake-survey-generator'
param redisCacheName string = 'redis-fake-survey-generator'
param sqlServerName string = 'sql-marcel-michau'
param sqlDatabaseName string = 'sqldb-fake-survey-generator'
param dnsZoneName string = 'mysecondarydomain.com'
param uiContainerVersion string
param apiContainerVersion string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' existing = {
  name: containerRegistryName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: managedIdentityName
}

resource redisCache 'Microsoft.Cache/Redis@2021-06-01' existing = {
  name: redisCacheName
}

resource sqlServer 'Microsoft.Sql/servers@2022-02-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-02-01-preview' existing = {
  name: sqlDatabaseName
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
    name: 'Cache__RedisUrl'
    value: redisCache.properties.hostName
  }
  {
    name: 'Cache__RedisPassword'
    value: redisCache.listKeys().primaryKey
  }
  {
    name: 'Cache__RedisDefaultDatabase'
    value: '0'
  }
  {
    name: 'Cache__RedisSsl'
    value: 'true'
  }
  {
    name: 'ConnectionStrings__SurveyContext'
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Managed Identity;User Id=${managedIdentity.properties.clientId};'
  }
  {
    name: 'IDENTITY_PROVIDER_URL'
    value: 'https://marcelmichau.eu.auth0.com/'
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
    daprConfig: {
      enabled: true
      appId: 'fake-survey-generator-api'
      appProtocol: 'http'
      appPort: 80
    }
  }
}

resource daprSecretStoreComponent 'Microsoft.App/managedEnvironments/daprComponents@2022-06-01-preview' = {
  name: 'dapr-secret-store'
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
