param location string = resourceGroup().location

@description('Name of the Azure Redis Cache')
param name string

@description('SKU/Tier of the Azure Redis Cache')
param sku object = {
  name: 'Basic'
  family: 'C'
  capacity: 0
}

resource redisCache 'Microsoft.Cache/redis@2022-06-01' = {
  name: name
  location: location
  properties: {
    sku: sku
    redisConfiguration: {}
    enableNonSslPort: false
  }
}
