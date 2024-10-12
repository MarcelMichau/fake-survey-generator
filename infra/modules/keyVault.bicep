@description('Location')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('Tenant id')
param tenantId string = subscription().tenantId

@description('The name of the Key Vault resource')
param name string

@description('Specifies all secrets {"secretName":"","secretValue":""} wrapped in a secure object.')
@secure()
param secretsObject object

@description('Subnet Resource ID for the infrastructure subnet')
param subnetResourceId string

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: name
  tags: tags
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: tenantId
    enabledForTemplateDeployment: false
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    networkAcls: {
      defaultAction: 'Deny'
      bypass: 'AzureServices'
      virtualNetworkRules: [
        {
          id: subnetResourceId
        }
      ]
    }
  }

  resource secrets 'secrets@2024-04-01-preview' = [
    for i in range(0, length(secretsObject.secrets)): {
      name: secretsObject.secrets[i].secretName
      properties: {
        value: secretsObject.secrets[i].secretValue
      }
    }
  ]
}

output keyVaultName string = keyVault.name
