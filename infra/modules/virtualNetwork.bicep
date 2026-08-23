@description('Name of the Virtual Network')
param name string

@description('Tags to apply to the resource')
param tags object

@description('Name of the compute subnet')
param subnetName string

@description('Name of the subnet used by deployment-script Azure Container Instances')
param deploymentScriptsSubnetName string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2025-05-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/22'
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: '10.0.0.0/27'
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
          delegations: [
            {
              name: 'container-apps-delegation'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
            }
          ]
        }
      }
      {
        name: deploymentScriptsSubnetName
        properties: {
          addressPrefix: '10.0.0.32/27'
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
            {
              service: 'Microsoft.Storage'
              locations: [
                'southafricanorth'
              ]
            }
          ]
          delegations: [
            {
              name: 'container-instance-delegation'
              properties: {
                serviceName: 'Microsoft.ContainerInstance/containerGroups'
              }
            }
          ]
        }
      }
    ]
  }
}

output subnetId string = virtualNetwork.properties.subnets[0].id
output deploymentScriptsSubnetId string = virtualNetwork.properties.subnets[1].id
