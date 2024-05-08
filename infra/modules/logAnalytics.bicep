@description('Name of the Log Analytics Workspace resource')
param name string

@description('Tags to apply to the resource')
param tags object

@description('Location of the Log Analytics Workspace resource')
param location string = resourceGroup().location

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  tags: tags
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

output name string = logAnalytics.name
output id string = logAnalytics.id
