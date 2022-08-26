@description('Name of the Azure Container Registry')
param name string

param location string = resourceGroup().location

@description('SKU/Tier of the Azure Container Registry')
param sku string = 'Basic'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' = {
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

output url string = containerRegistry.properties.loginServer
