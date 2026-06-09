targetScope = 'subscription'

param environment string
param location string

param applicationName string

param dnsZoneName string

@description('Subdomain label for rule-based routing custom domain (for example: app for app.contoso.com)')
param routeCustomDomainSubdomain string = 'fakesurveygeneratortest'

@description('Custom domain hostname for rule-based routing')
param routeCustomDomainName string = '${routeCustomDomainSubdomain}.${dnsZoneName}'

@description('Name of the environment HTTP route configuration')
param routeConfigName string = 'fakesurveygenerator'

@description('Route custom domain binding type')
@allowed([
  'Auto'
  'Disabled'
  'SniEnabled'
])
param routeCustomDomainBindingType string = 'Auto'

@description('Managed certificate resource name for the route custom domain')
param managedCertificateName string = 'fake-survey-generator-cert'

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
    principalId: managedIdentity.outputs.principalId
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
    routeConfigName: routeConfigName
    routeCustomDomainName: routeCustomDomainName
    routeCustomDomainBindingType: routeCustomDomainBindingType
    managedCertificateName: managedCertificateName
    apiContainerAppName: '${abbrs.appContainerApps}${applicationName}-api'
    uiContainerAppName: '${abbrs.appContainerApps}${applicationName}-ui'
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module dnsZone 'modules/dnsZone.bicep' = {
  name: 'dnsZone'
  params: {
    name: dnsZoneName
    tags: tags
    enableRouteDomainRecords: true
    routeDomainSubdomain: routeCustomDomainSubdomain
    routeDomainIpAddress: compute.outputs.containerAppEnvironmentStaticIp
    routeDomainVerificationId: compute.outputs.containerAppEnvironmentDomainVerificationId
  }
  scope: fakeSurveyGeneratorResourceGroup
}

output SERVICE_API_NAME string = '${abbrs.appContainerApps}${applicationName}-api'
output SERVICE_API_IDENTITY_NAME string = compute.outputs.managedIdentityName

output SERVICE_UI_NAME string = '${abbrs.appContainerApps}${applicationName}-ui'
output SERVICE_UI_IDENTITY_NAME string = compute.outputs.managedIdentityName

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.containerRegistryEndpoint
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = compute.outputs.containerAppEnvironmentId
output AZURE_REDIS_NAME string = redisCache.outputs.redisCacheName
output AZURE_APPLICATION_INSIGHTS_NAME string = applicationInsights.outputs.applicationInsightsName

output SQL_SERVER_NAME string = azureSql.outputs.sqlServerName
output SQL_DATABASE_NAME string = azureSql.outputs.sqlServerDatabaseName
