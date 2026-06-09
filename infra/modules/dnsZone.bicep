@description('Friendly name for the DNS Zone')
param name string

@description('Tags to apply to the resource')
param tags object

@description('Whether to create route custom domain A/TXT records for Container Apps')
param enableRouteDomainRecords bool = false

@description('DNS label for the custom-domain subdomain (for example: app for app.contoso.com)')
param routeDomainSubdomain string = ''

@description('Static inbound IP address of the Container Apps environment')
param routeDomainIpAddress string = ''

@description('Domain verification id used for TXT asuid.<subdomain> record')
param routeDomainVerificationId string = ''

var dnsRecordTimeToLive = 3600

resource dnsZone 'Microsoft.Network/dnsZones@2023-07-01-preview' = {
  name: name
  tags: tags
  location: 'global'
  properties: {
    zoneType: 'Public'
  }
}

resource routeDomainARecord 'Microsoft.Network/dnsZones/A@2023-07-01-preview' = if (enableRouteDomainRecords) {
  name: routeDomainSubdomain
  parent: dnsZone
  properties: {
    TTL: dnsRecordTimeToLive
    ARecords: [
      {
        ipv4Address: routeDomainIpAddress
      }
    ]
  }
}

resource routeDomainValidationTxtRecord 'Microsoft.Network/dnsZones/TXT@2023-07-01-preview' = if (enableRouteDomainRecords) {
  name: 'asuid.${routeDomainSubdomain}'
  parent: dnsZone
  properties: {
    TTL: dnsRecordTimeToLive
    TXTRecords: [
      {
        value: [
          routeDomainVerificationId
        ]
      }
    ]
  }
}

output name string = dnsZone.name
