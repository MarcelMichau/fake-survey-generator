@description('The name of the SQL logical server.')
param serverName string = uniqueString('sql', resourceGroup().id)

@description('The name of the SQL Database.')
param databaseName string = 'SampleDB'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The administrator username of the SQL logical server.')
param administratorLogin string

@description('The administrator password of the SQL logical server.')
@secure()
param administratorLoginPassword string

@description('The Azure AD administrator username of the SQL logical server.')
param azureAdAdministratorLogin string

@description('The Azure AD administrator Azure AD Object ID.')
param azureAdAdministratorObjectId string

@description('The Azure AD administrator Azure AD Tenant ID.')
param azureAdAdministratorTenantId string

resource sqlServer 'Microsoft.Sql/servers@2021-02-01-preview' = {
  name: serverName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-02-01-preview' = {
  parent: sqlServer
  name: databaseName
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  location: location
}

resource serverName_AllowAllAzureIps 'Microsoft.Sql/servers/firewallRules@2021-02-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

resource serverName_activeDirectory 'Microsoft.Sql/servers/administrators@2021-02-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: azureAdAdministratorLogin
    sid: azureAdAdministratorObjectId
    tenantId: azureAdAdministratorTenantId
  }
}
