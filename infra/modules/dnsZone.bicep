@description('Friendly name for the DNS Zone')
param name string

resource dnsZone 'Microsoft.Network/dnsZones@2023-07-01-preview' = {
  name: name
  location: 'global'
  properties: {
    zoneType: 'Public'
  }
}
