targetScope = 'subscription'

param location string = 'South Africa North'
param resourceGroupName string = 'rg-fake-survey-generator'
param logAnalyticsWorkspaceName string = 'log-fake-survey-generator'
param applicationInsightsName string = 'appi-fake-survey-generator'
param dnsZoneName string = 'mysecondarydomain.com'
param keyVaultName string = 'kv-fake-survey-generator'
param containerRegistryName string = 'acrfakesurveygenerator'
param redisCacheName string = 'redis-fake-survey-generator'
param sqlServerName string = 'sql-marcel-michau'
param sqlDatabaseName string = 'sqldb-fake-survey-generator'
param managedIdentityName string = 'mi-fake-survey-generator'
param containerAppEnvironmentName string = 'cae-fake-survey-generator'
param uiContainerAppName string = 'ca-fake-survey-generator-ui'
param apiContainerAppName string = 'ca-fake-survey-generator-api'
param sqlAzureAdAdministratorLogin string
param sqlAzureAdAdministratorObjectId string

resource fakeSurveyGeneratorResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics'
  params: {
    location: location
    name: logAnalyticsWorkspaceName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module applicationInsights 'modules/appInsights.bicep' = {
  name: 'applicationInsights'
  params: {
    location: location
    name: applicationInsightsName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module dnsZone 'modules/dnsZone.bicep' = {
  name: 'dnsZone'
  params: {
    name: dnsZoneName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault'
  params: {
    location: location
    name: keyVaultName
    secretsObject: {
      secrets: [
        {
          secretName: 'HealthCheckSecret'
          secretValue: 'healthy'
        }
      ]
    }
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module containerRegistry 'modules/containerRegistry.bicep' = {
  name: 'containerRegistry'
  params: {
    location: location
    name: containerRegistryName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module redisCache 'modules/redisCache.bicep' = {
  name: 'redisCache'
  params: {
    location: location
    name: redisCacheName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module azureSql 'modules/sql.bicep' = {
  name: 'azureSql'
  params: {
    location: location
    serverName: sqlServerName
    databaseName: sqlDatabaseName
    azureAdAdministratorLogin: sqlAzureAdAdministratorLogin
    azureAdAdministratorObjectId: sqlAzureAdAdministratorObjectId
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module managedIdentity 'modules/managedIdentity.bicep' = {
  name: 'managedIdentity'
  params: {
    location: location
    keyVaultName: keyVault.outputs.keyVaultName
    containerRegistryName: containerRegistry.outputs.containerRegistryName
    identityName: managedIdentityName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module uiContainerApp 'modules/containerApp.bicep' = {
  name: 'uiContainerApp'
  params: {
    location: location
    containerAppEnvName: containerAppEnvironmentName
    containerAppName: uiContainerAppName
    containerRegistryUrl: containerRegistry.outputs.url
    logAnalyticsName: logAnalytics.outputs.name
    identityType: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.outputs.identityResourceId}': {}
    }
    containerRegistryIdentity: managedIdentity.outputs.identityResourceId
    containers: [
      {
        name: 'fake-survey-generator-ui'
        image: '${containerRegistry.outputs.url}/fake-survey-generator-ui:4.3.728'
      }
    ]
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module apiContainerApp 'modules/containerApp.bicep' = {
  name: 'apiContainerApp'
  params: {
    location: location
    containerAppEnvName: containerAppEnvironmentName
    containerAppName: apiContainerAppName
    containerRegistryUrl: containerRegistry.outputs.url
    logAnalyticsName: logAnalytics.outputs.name
    identityType: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.outputs.identityResourceId}': {}
    }
    containerRegistryIdentity: managedIdentity.outputs.identityResourceId
    containers: [
      {
        name: 'fake-survey-generator-api'
        image: '${containerRegistry.outputs.url}/fake-survey-generator-api:3.5.327'
      }
    ]
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module frontDoor 'modules/frontDoor.bicep' = {
  name: 'frontDoor'
  params: {
    dnsZoneName: dnsZoneName
    uiOriginHostName: uiContainerApp.outputs.containerAppFqdn
    apiOriginHostName: apiContainerApp.outputs.containerAppFqdn
    cnameRecordName: 'fakesurveygenerator'
    endpointName: 'afd-fake-survey-generator'
  }
  scope: fakeSurveyGeneratorResourceGroup
}
