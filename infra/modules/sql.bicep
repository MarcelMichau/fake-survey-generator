@description('Tags to apply to the resource')
param tags object

@minLength(1)
@description('The id for the user-assigned managed identity that runs deploymentScripts')
param devOpsManagedIdentityId string

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

@description('Expecting the user-assigned managed identity that represents the API web app. Will become the SQL db admin')
param managedIdentity object

@minLength(1)
@description('The name of an admin account that can be used to add Managed Identities to Azure SQL')
param sqlAdministratorLogin string

@secure()
@minLength(1)
// note - this password should not be saved. the apps, and devs, connect with Managed Identity or Azure AD
@description('The password for an admin account that can be used to add Managed Identities to Azure SQL')
param sqlAdministratorPassword string

@description('Ensures that the idempotent scripts are executed each time the deployment is executed')
param uniqueScriptId string = newGuid()

resource allowSqlAdminScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'allowSqlAdminScript'
  location: location
  tags: tags
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${devOpsManagedIdentityId}': {}
    }
  }
  properties: {
    forceUpdateTag: uniqueScriptId
    azPowerShellVersion: '7.4'
    retentionInterval: 'P1D'
    cleanupPreference: 'OnSuccess'
    arguments: '-SqlServerName \'${serverName}\' -ResourceGroupName \'${resourceGroup().name}\''
    scriptContent: loadTextContent('../deploymentScripts/enableSqlAdminForServer.ps1')
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-02-01-preview' = {
  name: serverName
  tags: tags
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      // azureADOnlyAuthentication: true
      login: azureAdAdministratorLogin
      principalType: 'Group'
      sid: azureAdAdministratorObjectId
      tenantId: azureAdAdministratorTenantId
    }
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-02-01-preview' = {
  parent: sqlServer
  tags: tags
  name: databaseName
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  location: location
}

resource sqlServerVirtualNetworkRules 'Microsoft.Sql/servers/virtualNetworkRules@2023-02-01-preview' = {
  name: 'sql-vnet-rules'
  parent: sqlServer
  properties: {
    virtualNetworkSubnetId: subnetResourceId
  }
}

resource createSqlUserScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'createSqlUserScript'
  location: location
  tags: tags
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${devOpsManagedIdentityId}': {}
    }
  }
  properties: {
    forceUpdateTag: uniqueScriptId
    azPowerShellVersion: '7.4'
    retentionInterval: 'P1D'
    cleanupPreference: 'OnSuccess'
    arguments: '-ServerName \'${sqlServer.name}\' -ResourceGroupName \'${resourceGroup().name}\' -ServerUri \'${sqlServer.properties.fullyQualifiedDomainName}\' -DatabaseName \'${databaseName}\' -ApplicationId \'${managedIdentity.properties.principalId}\' -ManagedIdentityName \'${managedIdentity.name}\' -SqlAdminLogin \'${sqlAdministratorLogin}\' -SqlAdminPwd \'${sqlAdministratorPassword}\''
    scriptContent: loadTextContent('../deploymentScripts/createSqlAcctForManagedIdentity.ps1')
  }
  dependsOn: [
    sqlDatabase
  ]
}

output sqlServerInstance string = sqlServer.properties.fullyQualifiedDomainName
output sqlServerName string = sqlServer.name
output sqlServerDatabaseName string = sqlDatabase.name
