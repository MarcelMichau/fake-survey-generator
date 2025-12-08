@description('Tags to apply to the resource')
param tags object

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

@description('Managed Identity ID')
param managedIdentityId string

@description('Pipeline Identity Client ID')
param pipelineIdentityClientId string

resource sqlServer 'Microsoft.Sql/servers@2024-11-01-preview' = {
  name: serverName
  tags: tags
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

  resource sqlDatabase 'databases' = {
    tags: tags
    name: databaseName
    sku: {
      name: 'Basic'
      tier: 'Basic'
    }
    location: location
  }

  resource sqlServerVirtualNetworkRules 'virtualNetworkRules' = {
    name: 'sql-vnet-rules'
    properties: {
      virtualNetworkSubnetId: subnetResourceId
    }
  }
}

resource sqlDatabaseRoles 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: take('script-${uniqueString('sql_server', azureAdAdministratorLogin, 'database', resourceGroup().id)}', 24)
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  kind: 'AzurePowerShell'
  properties: {
    azPowerShellVersion: '14.4'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'DBNAME'
        value: databaseName
      }
      {
        name: 'DBSERVER'
        value: sqlServer.properties.fullyQualifiedDomainName
      }
      {
        name: 'PRINCIPALNAME'
        value: azureAdAdministratorLogin
      }
      {
        name: 'ID'
        value: azureAdAdministratorObjectId
      }
      {
        name: 'PIPELINEIDENTITYNAME'
        value: 'azure-devops'
      }
      {
        name: 'PIPELINEIDENTITYCLIENTID'
        value: pipelineIdentityClientId
      }
    ]
    scriptContent: loadTextContent('sql-deployment-script.ps1')
  }
}

output sqlServerInstance string = sqlServer.properties.fullyQualifiedDomainName
output sqlServerName string = sqlServer.name
output sqlServerDatabaseName string = sqlServer::sqlDatabase.name
