param containerAppName string
param containerAppEnvironmentId string
param containerAppEnvironmentName string
param containerRegistryUrl string
param imageName string
param managedIdentityName string
param sqlServerName string
param sqlDatabaseName string
param redisHostName string
param redisPasswordSecretUrl string
param applicationInsightsName string
param dnsZoneName string
param customDomainName string
param location string = resourceGroup().location
param version string

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-05-31-preview' existing = {
  name: managedIdentityName
}

resource sqlServer 'Microsoft.Sql/servers@2025-02-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2025-02-01-preview' existing = {
  parent: sqlServer
  name: sqlDatabaseName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource managedEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' existing = {
  name: containerAppEnvironmentName
}

resource dnsZone 'Microsoft.Network/dnsZones@2023-07-01-preview' existing = {
  name: dnsZoneName
}

var customDomainRecordName = split(customDomainName, '.')[0]

var apiEnvironmentVariables = [
  {
    name: 'AZURE_CLIENT_ID'
    value: managedIdentity.properties.clientId
  }
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
    name: 'Redis__HostName'
    value: redisHostName
  }
  {
    name: 'Redis__Password'
    secretRef: 'redis-password'
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

resource containerApp 'Microsoft.App/containerApps@2026-01-01' = {
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
      activeRevisionsMode: 'Single'
      secrets: [
        {
          name: 'redis-password'
          keyVaultUrl: redisPasswordSecretUrl
          identity: managedIdentity.id
        }
      ]
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
        customDomains: empty(customDomainName) ? [] : [
          {
            name: customDomainName
            bindingType: 'Auto'
          }
        ]
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
      revisionSuffix: replace(version, '.', '-')
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
    workloadProfileName: 'Consumption'
  }
}

resource validationTxtRecord 'Microsoft.Network/dnsZones/TXT@2023-07-01-preview' = if (!empty(customDomainName)) {
  parent: dnsZone
  name: 'asuid.${customDomainRecordName}'
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          containerApp.properties.customDomainVerificationId
        ]
      }
    ]
  }
}

resource managedCertificate 'Microsoft.App/managedEnvironments/managedCertificates@2026-01-01' = if (!empty(customDomainName)) {
  parent: managedEnvironment
  name: '${managedEnvironment.name}-${customDomainRecordName}-certificate'
  location: location
  properties: {
    subjectName: customDomainName
    domainControlValidation: 'CNAME'
  }
  dependsOn: [
    validationTxtRecord
  ]
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
