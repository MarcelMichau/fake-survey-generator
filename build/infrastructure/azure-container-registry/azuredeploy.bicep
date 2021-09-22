param name string
param location string = resourceGroup().location
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
