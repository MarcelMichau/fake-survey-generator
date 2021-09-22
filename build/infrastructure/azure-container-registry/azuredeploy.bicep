@description('Name of the Azure Container Registry')
param name string

param location string = resourceGroup().location

@description('SKU/Tier of the Azure Container Registry')
param sku string = 'Basic'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2020-11-01-preview' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
  dependsOn: []
}
