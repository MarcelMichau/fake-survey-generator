param location string = resourceGroup().location
param name string
param sku object = {
  name: 'Basic'
  family: 'C'
  capacity: 0
}

resource redisCache 'Microsoft.Cache/Redis@2020-12-01' = {
  name: name
  location: location
  properties: {
    sku: sku
    redisConfiguration: {}
    enableNonSslPort: false
  }
}
