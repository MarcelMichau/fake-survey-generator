param tags object
param location string
param containerAppEnvironmentName string
param managedIdentityName string
param logAnalyticsName string
param virtualNetworkSubnetId string

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: managedIdentityName
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

output containerAppEnvironmentFqdn string = containerAppEnvironment.outputs.fqdn
output containerAppEnvironmentId string = containerAppEnvironment.outputs.containerAppEnvironmentId
output containerAppEnvironmentName string = containerAppEnvironment.outputs.containerAppEnvironmentName
output containerAppEnvironmentDefaultDomain string = containerAppEnvironment.outputs.defaultDomain

output managedIdentityName string = managedIdentity.name
output managedIdentityPrincipalId string = managedIdentity.properties.principalId
