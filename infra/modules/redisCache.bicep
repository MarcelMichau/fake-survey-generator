param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('Name of the Azure Redis Cache')
param name string

@description('SKU/Tier of the Azure Redis Cache')
param sku object = {
  name: 'Basic'
  family: 'C'
  capacity: 0
}

resource redisCache 'Microsoft.Cache/redis@2024-04-01-preview' = {
  name: name
  tags: tags
  location: location
  properties: {
    sku: sku
    redisConfiguration: {}
    enableNonSslPort: false
    redisVersion: '6'
  }
}

output redisCacheName string = redisCache.name
