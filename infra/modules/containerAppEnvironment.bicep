@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('Specifies the name of the Container App Environment')
param containerAppEnvName string

@description('Specifies the name of the log analytics workspace')
param logAnalyticsName string

@description('Subnet Resource ID for the infrastructure subnet')
param subnetResourceId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: logAnalyticsName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2025-02-02-preview' = {
  name: containerAppEnvName
  location: location
  tags: tags
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

output containerAppEnvironmentId string = containerAppEnvironment.id
output containerAppEnvironmentName string = containerAppEnvironment.name
