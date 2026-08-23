param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('User Assigned Managed Identity Name')
param identityName string

@description('Key Vault to assign roles to')
param keyVaultName string

@description('Container Registry to assign roles to')
param containerRegistryName string

var keyVaultSecretsUser = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4633458b-17de-408a-b874-0445c86b69e6'
)
var uniqueRoleGuidKeyVaultSecretsUser = guid(
  resourceId('Microsoft.KeyVault/vaults', keyVaultName),
  keyVaultSecretsUser
)

var acrPull = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var uniqueRoleGuidAcrPull = guid(resourceId('Microsoft.ContainerRegistry/registries', containerRegistryName), acrPull)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-05-31-preview' = {
  name: identityName
  tags: tags
  location: location
}

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
}

resource keyVaultSecretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: uniqueRoleGuidKeyVaultSecretsUser
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultSecretsUser
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2026-01-01-preview' existing = {
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
output principalId string = managedIdentity.properties.principalId
output tenantId string = managedIdentity.properties.tenantId
