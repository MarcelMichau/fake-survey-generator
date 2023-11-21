targetScope = 'subscription'

param environment string
param location string

param applicationName string

param dnsZoneName string

param sqlAzureAdAdministratorLogin string
param sqlAzureAdAdministratorObjectId string

@description('Specifies if the API app exists')
param apiAppExists bool = false

@description('Specifies if the UI app exists')
param uiAppExists bool = false

var tags = { 'azd-env-name': environment }

var abbrs = loadJsonContent('abbreviations.json')

resource fakeSurveyGeneratorResourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: '${abbrs.resourcesResourceGroups}${applicationName}'
  location: location
}

var computeSubnetName = 'container-app'

module virtualNetwork 'modules/virtualNetwork.bicep' = {
  name: 'virtualNetwork'
  params: {
    location: location
    name: '${abbrs.networkVirtualNetworks}${applicationName}'
    tags: tags
    subnetName: computeSubnetName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics'
  params: {
    location: location
    name: '${abbrs.operationalInsightsWorkspaces}${applicationName}'
    tags: tags
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module applicationInsights 'modules/appInsights.bicep' = {
  name: 'applicationInsights'
  params: {
    location: location
    name: '${abbrs.insightsComponents}${applicationName}'
    tags: tags
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module applicationInsightsDashboard 'modules/applicationInsightsDashboard.bicep' = {
  name: 'application-insights-dashboard'
  params: {
    name: '${abbrs.portalDashboards}${applicationName}'
    location: location
    tags: tags
    applicationInsightsName: applicationInsights.name
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module dnsZone 'modules/dnsZone.bicep' = {
  name: 'dnsZone'
  params: {
    name: dnsZoneName
    tags: tags
  }
  scope: fakeSurveyGeneratorResourceGroup
}

var azureSqlPassword = 'C0mpl3x!ity-${uniqueString(subscription().id, fakeSurveyGeneratorResourceGroup.id, '${abbrs.sqlServers}${applicationName}')}'

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault'
  params: {
    location: location
    name: '${abbrs.keyVaultVaults}${applicationName}'
    tags: tags
    secretsObject: {
      secrets: [
        {
          secretName: 'HealthCheckSecret'
          secretValue: 'healthy'
        }
        {
          secretName: 'SqlAdminPassword'
          secretValue: azureSqlPassword
        }
      ]
    }
    subnetResourceId: virtualNetwork.outputs.subnetId
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module containerRegistry 'modules/containerRegistry.bicep' = {
  name: 'containerRegistry'
  params: {
    location: location
    name: replace('${abbrs.containerRegistryRegistries}${applicationName}', '-', '')
    tags: tags
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module redisCache 'modules/redisCache.bicep' = {
  name: 'redisCache'
  params: {
    location: location
    name: '${abbrs.cacheRedis}${applicationName}'
    tags: tags
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module azureSql 'modules/sql.bicep' = {
  name: 'azureSql'
  params: {
    location: location
    tags: tags
    serverName: '${abbrs.sqlServers}${applicationName}'
    databaseName: '${abbrs.sqlServersDatabases}${applicationName}'
    azureAdAdministratorLogin: sqlAzureAdAdministratorLogin
    azureAdAdministratorObjectId: sqlAzureAdAdministratorObjectId
    subnetResourceId: virtualNetwork.outputs.subnetId
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module managedIdentity 'modules/managedIdentity.bicep' = {
  name: 'managedIdentity'
  params: {
    location: location
    tags: tags
    keyVaultName: keyVault.outputs.keyVaultName
    containerRegistryName: containerRegistry.outputs.containerRegistryName
    identityName: '${abbrs.managedIdentityUserAssignedIdentities}${applicationName}'
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module compute 'compute.bicep' = {
  name: 'compute'
  params: {
    location: location
    tags: tags
    apiContainerAppName: '${abbrs.appContainerApps}${applicationName}-api'
    apiContainerAppExists: apiAppExists
    applicationInsightsName: applicationInsights.outputs.applicationInsightsName
    containerAppEnvironmentName: '${abbrs.appManagedEnvironments}${applicationName}'
    logAnalyticsName: logAnalytics.outputs.name
    virtualNetworkSubnetId: virtualNetwork.outputs.subnetId
    containerRegistryName: containerRegistry.outputs.containerRegistryName
    managedIdentityName: managedIdentity.outputs.identityName
    redisCacheName: redisCache.outputs.redisCacheName
    sqlDatabaseName: azureSql.outputs.sqlServerDatabaseName
    sqlServerName: azureSql.outputs.sqlServerName
    uiContainerAppName: '${abbrs.appContainerApps}${applicationName}-ui'
    uiContainerAppExists: uiAppExists
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module frontDoor 'modules/frontDoor.bicep' = {
  name: 'frontDoor'
  params: {
    tags: tags
    dnsZoneName: dnsZone.outputs.name
    uiOriginHostName: compute.outputs.uiContainerAppFqdn
    apiOriginHostName: compute.outputs.apiContainerAppFqdn
    cnameRecordName: replace(applicationName, '-', '')
    endpointName: '${abbrs.networkFrontDoors}${applicationName}'
  }
  scope: fakeSurveyGeneratorResourceGroup
}

output SERVICE_API_IMAGE_NAME string = compute.outputs.apiContainerImageName
output SERVICE_API_NAME string = compute.outputs.apiContainerAppName
output SERVICE_API_URI string = compute.outputs.apiContainerAppUri
output SERVICE_API_IDENTITY_NAME string = compute.outputs.apiContainerAppIdentityName
output SERVICE_API_IDENTITY_PRINCIPAL_ID string = compute.outputs.apiContainerAppIdentityPrincipalId

output SERVICE_UI_IMAGE_NAME string = compute.outputs.uiContainerImageName
output SERVICE_UI_NAME string = compute.outputs.uiContainerAppName
output SERVICE_UI_URI string = compute.outputs.uiContainerAppUri
output SERVICE_UI_IDENTITY_NAME string = compute.outputs.uiContainerAppIdentityName
output SERVICE_UI_IDENTITY_PRINCIPAL_ID string = compute.outputs.uiContainerAppIdentityPrincipalId

output AZURE_RESOURCE_GROUP_NAME string = fakeSurveyGeneratorResourceGroup.name

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.containerRegistryEndpoint
output AZURE_CONTAINER_REGISTRY_NAME string = containerRegistry.outputs.containerRegistryName

output SQL_SERVER_INSTANCE string = azureSql.outputs.sqlServerInstance
output SQL_SERVER_NAME string = azureSql.outputs.sqlServerName
output SQL_DATABASE_NAME string = azureSql.outputs.sqlServerDatabaseName
