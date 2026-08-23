@description('Friendly name for the DNS Zone')
param name string

@description('Tags to apply to the resource')
param tags object

@description('The name of the application CNAME record within the DNS zone')
param cnameRecordName string

@description('The hostname targeted by the application CNAME record')
param cnameTargetHostName string

resource dnsZone 'Microsoft.Network/dnsZones@2023-07-01-preview' = {
  name: name
  tags: tags
  location: 'global'
  properties: {
    zoneType: 'Public'
  }

  resource cnameRecord 'CNAME' = {
    name: cnameRecordName
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: cnameTargetHostName
      }
    }
  }
}

output name string = dnsZone.name
