@description('The name of the SQL logical server')
param serverName string = uniqueString('sql', resourceGroup().id)

@description('The name of the SQL Database')
param databaseName string = 'SampleDB'

@description('Location for all resources')
param location string = resourceGroup().location

@description('The Azure AD administrator username of the SQL logical server')
param azureAdAdministratorLogin string

@description('The Azure AD administrator Azure AD Object ID')
param azureAdAdministratorObjectId string

@description('The Azure AD administrator Azure AD Tenant ID')
param azureAdAdministratorTenantId string = subscription().tenantId

@description('Subnet Resource ID for the infrastructure subnet')
param subnetResourceId string

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: serverName
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: azureAdAdministratorLogin
      principalType: 'Group'
      sid: azureAdAdministratorObjectId
      tenantId: azureAdAdministratorTenantId
    }
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-11-01-preview' = {
  parent: sqlServer
  name: databaseName
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  location: location
}

resource sqlServerVirtualNetworkRules 'Microsoft.Sql/servers/virtualNetworkRules@2022-05-01-preview' = {
  name: 'sql-vnet-rules'
  parent: sqlServer
  properties: {
    virtualNetworkSubnetId: subnetResourceId
  }
}
