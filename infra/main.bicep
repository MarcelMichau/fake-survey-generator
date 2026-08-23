targetScope = 'subscription'

param environment string
param location string

param applicationName string

param dnsZoneName string

var tags = { 'azd-env-name': environment }

var abbrs = loadJsonContent('abbreviations.json')

resource fakeSurveyGeneratorResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: '${abbrs.resourcesResourceGroups}${applicationName}'
  location: location
  tags: tags
}

module virtualNetwork 'modules/virtualNetwork.bicep' = {
  name: 'virtualNetwork'
  params: {
    location: location
    name: '${abbrs.networkVirtualNetworks}${applicationName}'
    tags: tags
    subnetName: '${abbrs.networkVirtualNetworksSubnets}container-apps'
    deploymentScriptsSubnetName: '${abbrs.networkVirtualNetworksSubnets}deployment-scripts'
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

module dnsZone 'modules/dnsZone.bicep' = {
  name: 'dnsZone'
  params: {
    name: dnsZoneName
    tags: tags
    cnameRecordName: replace(applicationName, '-', '')
    cnameTargetHostName: '${abbrs.appContainerApps}${applicationName}-api.${compute.outputs.containerAppEnvironmentDefaultDomain}'
  }
  scope: fakeSurveyGeneratorResourceGroup
}

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
      ]
    }
    subnetResourceIds: [
      virtualNetwork.outputs.subnetId
      virtualNetwork.outputs.deploymentScriptsSubnetId
    ]
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module deploymentScriptStorage 'modules/deploymentScriptStorage.bicep' = {
  name: 'deploymentScriptStorage'
  params: {
    location: location
    tags: tags
    name: take(toLower('${abbrs.storageStorageAccounts}${uniqueString('deployment-scripts', fakeSurveyGeneratorResourceGroup.id)}'), 24)
    subnetResourceId: virtualNetwork.outputs.deploymentScriptsSubnetId
    deploymentScriptIdentityPrincipalId: managedIdentity.outputs.principalId
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

module redisPassword 'modules/redisPassword.bicep' = {
  name: 'redisPassword'
  params: {
    location: location
    tags: tags
    name: '${abbrs.managedIdentityUserAssignedIdentities}${applicationName}-redis-password-generator'
    keyVaultName: keyVault.outputs.keyVaultName
    deploymentScriptStorageAccountName: deploymentScriptStorage.outputs.name
    deploymentScriptSubnetResourceId: virtualNetwork.outputs.deploymentScriptsSubnetId
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module redisCache 'modules/redisContainerApp.bicep' = {
  name: 'redisCache'
  params: {
    name: '${abbrs.appContainerApps}${applicationName}-redis'
    tags: tags
    containerAppEnvironmentId: compute.outputs.containerAppEnvironmentId
    managedIdentityName: managedIdentity.outputs.identityName
    redisPasswordSecretUrl: redisPassword.outputs.secretUrl
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
    azureAdAdministratorLogin: managedIdentity.outputs.identityName
    azureAdAdministratorObjectId: managedIdentity.outputs.principalId
    subnetResourceId: virtualNetwork.outputs.subnetId
    deploymentScriptStorageAccountName: deploymentScriptStorage.outputs.name
    deploymentScriptSubnetResourceId: virtualNetwork.outputs.deploymentScriptsSubnetId
    managedIdentityId: managedIdentity.outputs.identityResourceId
    pipelineIdentityClientId: 'df54403d-edea-442f-bc25-99403859119c'
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

module compute 'modules/compute.bicep' = {
  name: 'compute'
  params: {
    location: location
    tags: tags
    containerAppEnvironmentName: '${abbrs.appManagedEnvironments}${applicationName}'
    logAnalyticsName: logAnalytics.outputs.name
    virtualNetworkSubnetId: virtualNetwork.outputs.subnetId
    managedIdentityName: managedIdentity.outputs.identityName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

output SERVICE_API_NAME string = '${abbrs.appContainerApps}${applicationName}-api'
output SERVICE_API_IDENTITY_NAME string = compute.outputs.managedIdentityName
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = compute.outputs.containerAppEnvironmentName
output DNS_ZONE_NAME string = dnsZoneName
output CUSTOM_DOMAIN_NAME string = '${replace(applicationName, '-', '')}.${dnsZoneName}'

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.containerRegistryEndpoint
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = compute.outputs.containerAppEnvironmentId
output REDIS_CACHE_HOST_NAME string = redisCache.outputs.hostName
output REDIS_KEY_VAULT_URL string = redisPassword.outputs.secretUrl
output AZURE_APPLICATION_INSIGHTS_NAME string = applicationInsights.outputs.applicationInsightsName

output SQL_SERVER_NAME string = azureSql.outputs.sqlServerName
output SQL_DATABASE_NAME string = azureSql.outputs.sqlServerDatabaseName
