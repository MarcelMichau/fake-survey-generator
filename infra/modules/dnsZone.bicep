@description('Friendly name for the DNS Zone')
param name string

resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: name
  location: 'global'
  properties: {
    zoneType: 'Public'
  }
}
