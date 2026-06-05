param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('Name of the Azure Managed Redis cluster')
param name string

@description('Principal ID of the managed identity to grant Redis access to')
param principalId string

resource redisEnterprise 'Microsoft.Cache/redisEnterprise@2025-07-01' = {
  name: name
  tags: tags
  location: location
  sku: {
    name: 'Balanced_B0'
  }
  properties: {
    highAvailability: 'Disabled'
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource redisEnterpriseDatabase 'Microsoft.Cache/redisEnterprise/databases@2025-07-01' = {
  parent: redisEnterprise
  name: 'default'
  properties: {
    accessKeysAuthentication: 'Disabled'
    clientProtocol: 'Encrypted'
    port: 10000
  }
}

resource redisAccessPolicyAssignment 'Microsoft.Cache/redisEnterprise/databases/accessPolicyAssignments@2025-07-01' = {
  parent: redisEnterpriseDatabase
  name: 'managedIdentityAccess'
  properties: {
    accessPolicyName: 'default'
    user: {
      objectId: principalId
    }
  }
}

output redisCacheName string = redisEnterprise.name
