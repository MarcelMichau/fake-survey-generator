@description('<USER ASSIGNED IDENTITY NAME>')
param identityName string

@description('<KEY VAULT RESOURCE GROUP>')
param keyVaultResourceGroup string

@description('<KEY VAULT NAME>')
param keyVaultName string

var keyVaultSecretsOfficer = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: identityName
  location: resourceGroup().location
}

resource keyVaultSecretsOfficerRoleAssignment 'Microsoft.KeyVault/vaults/providers/roleAssignments@2018-01-01-preview' = if (isKeyVaultEnabled) {
  name: '${keyVaultName}/Microsoft.Authorization/${uniqueRoleGuidKeyVaultSecretsOfficer}'
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer
    principalId: reference(resourceId('Microsoft.ManagedIdentity/userAssignedIdentities/', identityName), '2018-11-30').principalId
    scope: resourceId('Microsoft.KeyVault/vaults', keyVaultName)
    principalType: 'ServicePrincipal'
  }
}

output identityName string = identityName
output identityResourceId string = managedIdentity.id
output identityClientId string = reference(managedIdentity.id, '2018-11-30', 'Full').properties.clientId