param containerAppName string
param containerAppEnvironmentId string
param containerRegistryUrl string
param imageName string
param managedIdentityName string
param location string = resourceGroup().location

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: managedIdentityName
}

resource containerApp 'Microsoft.App/containerApps@2025-10-02-preview' = {
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
