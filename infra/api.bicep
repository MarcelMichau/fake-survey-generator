param containerAppName string
param containerAppEnvironmentId string
param containerRegistryUrl string
param imageName string
param managedIdentityName string
param sqlServerName string
param sqlDatabaseName string
param redisCacheName string
param applicationInsightsName string
param location string = resourceGroup().location

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: managedIdentityName
}

resource redisCache 'Microsoft.Cache/redis@2024-11-01' existing = {
  name: redisCacheName
}

resource sqlServer 'Microsoft.Sql/servers@2024-11-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2024-11-01-preview' existing = {
  parent: sqlServer
  name: sqlDatabaseName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

var apiEnvironmentVariables = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
    value: 'true'
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

resource containerApp 'Microsoft.App/containerApps@2025-10-02-preview' = {
  name: containerAppName
  location: location
  tags: {
    'azd-service-name': 'api'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    configuration: {
      registries: [
        {
          server: containerRegistryUrl
          identity: managedIdentity.id
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      dapr: {
        enabled: true
        appId: 'fake-survey-generator-api'
        appProtocol: 'http'
        appPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'fake-survey-generator-api'
          image: imageName
          env: apiEnvironmentVariables
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
