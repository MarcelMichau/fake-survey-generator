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
param productionRevisionName string = ''

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: managedIdentityName
}

var previewLabel = activeLabel == 'blue' ? 'green' : 'blue'
var productionLabel = promotePreview ? previewLabel : activeLabel
var targetLabel = promotePreview ? productionLabel : previewLabel
var useLatestForProductionTraffic = promotePreview || empty(productionRevisionName)
var previewTrafficEntry = {
  label: previewLabel
  latestRevision: true
  weight: 0
}
var productionTrafficEntry = useLatestForProductionTraffic
  ? {
      label: productionLabel
      latestRevision: true
      weight: 100
    }
  : {
      label: productionLabel
      revisionName: productionRevisionName
      weight: 100
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
      activeRevisionsMode: 'Labels'
      targetLabel: targetLabel
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
        traffic: promotePreview ? [
          productionTrafficEntry
        ] : [
          previewTrafficEntry
          productionTrafficEntry
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
