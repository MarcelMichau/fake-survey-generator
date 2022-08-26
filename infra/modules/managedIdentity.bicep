param location string = resourceGroup().location

@description('User Assigned Managed Identity Name')
param identityName string

@description('Key Vault to assign roles to')
param keyVaultName string

@description('Container Registry to assign roles to')
param containerRegistryName string

var keyVaultSecretsOfficer = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
var uniqueRoleGuidKeyVaultSecretsOfficer = guid(resourceId('Microsoft.KeyVault/vaults', keyVaultName), keyVaultSecretsOfficer)

var acrPull = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var uniqueRoleGuidAcrPull = guid(resourceId('Microsoft.ContainerRegistry/registries', containerRegistryName), acrPull)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: identityName
  location: location
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource keyVaultSecretsOfficerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: uniqueRoleGuidKeyVaultSecretsOfficer
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' existing = {
  name: containerRegistryName
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: uniqueRoleGuidAcrPull
  scope: containerRegistry
  properties: {
    roleDefinitionId: acrPull
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

output identityName string = identityName
output identityResourceId string = managedIdentity.id
output identityClientId string = managedIdentity.properties.clientId
