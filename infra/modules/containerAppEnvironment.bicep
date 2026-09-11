@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('Specifies the name of the Container App Environment')
param containerAppEnvName string

@description('Specifies the name of the Log Analytics workspace used by Azure Monitor diagnostic settings')
param logAnalyticsName string

@description('Subnet Resource ID for the infrastructure subnet')
param subnetResourceId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-07-01' existing = {
  name: logAnalyticsName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: containerAppEnvName
  location: location
  tags: tags
  properties: {
    // Use the Azure Monitor resource-log pipeline instead of the legacy
    // Log Analytics HTTP Data Collector API. The latter creates the classic
    // *_CL tables, which cannot be migrated when they contain service-owned
    // columns such as _timestamp_d.
    appLogsConfiguration: {
      destination: 'azure-monitor'
    }
    vnetConfiguration: {
      infrastructureSubnetId: subnetResourceId
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }

}

// Route the Azure Monitor Container Apps resource logs to the workspace.
// The resource-specific tables are ContainerAppConsoleLogs and
// ContainerAppSystemLogs (without the legacy _CL suffix).
resource containerAppLogs 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: containerAppEnvironment
  name: 'container-app-logs'
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      {
        category: 'ContainerAppConsoleLogs'
        enabled: true
      }
      {
        category: 'ContainerAppSystemLogs'
        enabled: true
      }
    ]
  }
}

output containerAppEnvironmentId string = containerAppEnvironment.id
output containerAppEnvironmentName string = containerAppEnvironment.name
output fqdn string = containerAppEnvironment.properties.defaultDomain
output defaultDomain string = containerAppEnvironment.properties.defaultDomain
output customDomainVerificationId string = containerAppEnvironment.properties.customDomainConfiguration.customDomainVerificationId
