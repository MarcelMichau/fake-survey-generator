@description('Name of the storage account used by VNet-connected deployment scripts')
param name string

@description('Tags to apply to the resource')
param tags object

@description('Subnet Resource ID used by deployment-script Azure Container Instances')
param subnetResourceId string

@description('Principal ID of the user-assigned managed identity used by deployment scripts')
param deploymentScriptIdentityPrincipalId string

@description('Location for all resources')
param location string = resourceGroup().location

var storageFileDataPrivilegedContributor = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69566ab7-960f-475b-8e7c-b3118f30c6bd'
)

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          id: subnetResourceId
          action: 'Allow'
        }
      ]
    }
  }
}

resource storageFileDataPrivilegedContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, deploymentScriptIdentityPrincipalId, storageFileDataPrivilegedContributor)
  scope: storageAccount
  properties: {
    roleDefinitionId: storageFileDataPrivilegedContributor
    principalId: deploymentScriptIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output name string = storageAccount.name
