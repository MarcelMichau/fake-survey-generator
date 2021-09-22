@description('Managed Identity Name used by the AKS Cluster')
param aksClusterManagedIdentityName string

@description('Resource Group Name which contains all the nodes in the AKS Cluster')
param aksClusterNodeResourceGroupName string

@description('Name of the Azure Container Registry for which the AKS Cluster Managed Identity requires AcrPull role')
param azureContainerRegistryName string

var managedIdentityOperatorRole = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/f1a07417-d97a-45cb-824c-7a7467783830'
var acrPullRole = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource managedIdentityOperatorRoleAssignment 'Microsoft.Authorization/roleAssignments@2017-09-01' = {
  name: guid('${resourceGroup().id}${aksClusterManagedIdentityName}Managed Identity Operator')
  properties: {
    roleDefinitionId: managedIdentityOperatorRole
    principalId: reference(resourceId(aksClusterNodeResourceGroupName, 'Microsoft.ManagedIdentity/userAssignedIdentities', aksClusterManagedIdentityName), '2018-11-30').principalId
    scope: resourceGroup().id
  }
}

resource acrPullRoleAssignment 'Microsoft.ContainerRegistry/registries/providers/roleAssignments@2018-09-01-preview' = {
  name: '${azureContainerRegistryName}/Microsoft.Authorization/${guid(uniqueString('${aksClusterManagedIdentityName}${acrPullRole}'))}'
  properties: {
    roleDefinitionId: acrPullRole
    principalId: reference(resourceId(aksClusterNodeResourceGroupName, 'Microsoft.ManagedIdentity/userAssignedIdentities', aksClusterManagedIdentityName), '2018-11-30').principalId
    scope: resourceId('Microsoft.ContainerRegistry/registries', azureContainerRegistryName)
    principalType: 'ServicePrincipal'
  }
}
