@description('Managed Identity Name used by the AKS Cluster')
param aksClusterManagedIdentityName string

var managedIdentityOperatorRole = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/f1a07417-d97a-45cb-824c-7a7467783830'
var virtualMachineContributorRole = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/9980e02c-c2be-4d73-94e8-173b1dc7cf3c'

resource managedIdentityOperatorRoleAssignment 'Microsoft.Authorization/roleAssignments@2017-09-01' = {
  name: guid('${resourceGroup().id}${aksClusterManagedIdentityName}Managed Identity Operator')
  properties: {
    roleDefinitionId: managedIdentityOperatorRole
    principalId: reference(resourceId('Microsoft.ManagedIdentity/userAssignedIdentities', aksClusterManagedIdentityName), '2018-11-30').principalId
    scope: resourceGroup().id
  }
}

resource virtualMachineContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2017-09-01' = {
  name: guid('${resourceGroup().id}${aksClusterManagedIdentityName}Virtual Machine Contributor')
  properties: {
    roleDefinitionId: virtualMachineContributorRole
    principalId: reference(resourceId('Microsoft.ManagedIdentity/userAssignedIdentities', aksClusterManagedIdentityName), '2018-11-30').principalId
    scope: resourceGroup().id
  }
}