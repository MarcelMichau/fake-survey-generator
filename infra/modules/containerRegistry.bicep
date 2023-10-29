@description('Name of the Azure Container Registry')
param name string

param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('SKU/Tier of the Azure Container Registry')
param sku string = 'Basic'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-08-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
  dependsOn: []
}

output containerRegistryEndpoint string = containerRegistry.properties.loginServer
output containerRegistryName string = containerRegistry.name
