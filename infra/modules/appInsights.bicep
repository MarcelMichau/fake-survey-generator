@description('Name of the Application Insights resource')
param name string

@description('Location of the Application Insights resource')
param location string = resourceGroup().location

@description('Log Analytics Workspace ID to send telemetry to')
param logAnalyticsWorkspaceId string

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
  }
}

output connectionString string = applicationInsights.properties.ConnectionString
