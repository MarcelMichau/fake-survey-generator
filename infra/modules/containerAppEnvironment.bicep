@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('Specifies the name of the Container App Environment')
param containerAppEnvName string

@description('Specifies the name of the log analytics workspace')
param logAnalyticsName string

@description('Subnet Resource ID for the infrastructure subnet')
param subnetResourceId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: logAnalyticsName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-02-preview' = {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: subnetResourceId
    }
  }
}
