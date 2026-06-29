@description('Friendly name for the DNS Zone')
param name string

@description('Tags to apply to the resource')
param tags object

resource dnsZone 'Microsoft.Network/dnsZones@2023-07-01-preview' = {
  name: name
  tags: tags
  location: 'global'
  properties: {
    zoneType: 'Public'
  }
}

output name string = dnsZone.name
