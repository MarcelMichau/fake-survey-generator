@description('Specifies the name of the Container App')
param containerAppName string

@description('Specifies the name of the Container App Environment')
param containerAppEnvName string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('Whether or not traffic is allowed from outside the Container App Environment')
param externalIngressEnabled bool = true

@description('Specifies the container port')
param targetPort int = 80

@description('Azure Container Registry Url')
param containerRegistryUrl string

@description('Containers to run - https://docs.microsoft.com/en-us/azure/templates/microsoft.app/containerapps?pivots=deployment-language-bicep#container')
param containers array = []

@description('Minimum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param minReplicas int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param maxReplicas int = 1

param identityType string = 'SystemAssigned'

@description('User Assigned Managed Identities to associate with the Container App')
param userAssignedIdentities object = {}

@description('Identity to use to authenticate with Azure Container Registry')
param containerRegistryIdentity string = 'system'

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' existing = {
  name: containerAppEnvName
}

resource containerApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: identityType
    userAssignedIdentities: empty(userAssignedIdentities) ? null : userAssignedIdentities
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      registries: [
        {
          server: containerRegistryUrl
          identity: containerRegistryIdentity
        }
      ]
      ingress: {
        external: externalIngressEnabled
        targetPort: targetPort
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
      containers: empty(containers) ? null : containers
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
