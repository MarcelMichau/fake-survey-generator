targetScope = 'subscription'

@description('Name of the Resource Group to create')
param resourceGroupName string = 'rg-test'

@description('Location in which to create the Resource Group')
param location string = 'location'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: {}
  properties: {}
}
