param containerAppName string
param containerAppEnvironmentId string
param containerRegistryUrl string
param imageName string
param managedIdentityName string
param location string = resourceGroup().location
param version string

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-05-31-preview' existing = {
  name: managedIdentityName
}

resource containerApp 'Microsoft.App/containerApps@2026-01-01' = {
  name: containerAppName
  location: location
  tags: {
    'azd-service-name': 'ui'
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
      registries: [
        {
          server: containerRegistryUrl
          identity: managedIdentity.id
        }
      ]
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
    }
    template: {
      revisionSuffix: replace(version, '.', '-')
      containers: [
        {
          name: 'fake-survey-generator-ui'
          image: imageName
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
