param containerAppName string
param containerAppEnvironmentId string
param containerRegistryUrl string
param imageName string
param managedIdentityName string
param location string = resourceGroup().location
param version string
@allowed([
  'blue'
  'green'
])
param activeLabel string = 'blue'
param promotePreview bool = false

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: managedIdentityName
}

var previewLabel = activeLabel == 'blue' ? 'green' : 'blue'
var productionLabel = promotePreview ? previewLabel : activeLabel

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
      activeRevisionsMode: 'Labels'
      targetLabel: productionLabel
      maxInactiveRevisions: 5
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
            label: previewLabel
            latestRevision: true
            weight: 0
          }
          {
            label: productionLabel
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
