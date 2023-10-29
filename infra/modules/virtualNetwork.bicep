@description('Name of the Virtual Network')
param name string

@description('Tags to apply to the resource')
param tags object

@description('Name of the compute subnet')
param subnetName string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: '10.0.0.0/23'
          serviceEndpoints: [
            {
              service: 'Microsoft.Sql'
              locations: [
                'southafricanorth'
              ]
            }
            {
              service: 'Microsoft.KeyVault'
              locations: [
                'southafricanorth'
              ]
            }
          ]
        }
      }
    ]
  }
}

output subnetId string = virtualNetwork.properties.subnets[0].id
