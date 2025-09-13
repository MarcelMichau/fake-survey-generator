@description('Tags to apply to the resource')
param tags object

@description('The name of the Front Door endpoint to create. This must be globally unique.')
param endpointName string = 'afd-${uniqueString(resourceGroup().id)}'

@description('The name of the SKU to use when creating the Front Door profile.')
@allowed([
  'Standard_AzureFrontDoor'
  'Premium_AzureFrontDoor'
])
param skuName string = 'Standard_AzureFrontDoor'

@description('The UI host name that should be used when connecting from Front Door to the origin.')
param uiOriginHostName string

@description('The API host name that should be used when connecting from Front Door to the origin.')
param apiOriginHostName string

@description('The name of the DNS Zone')
param dnsZoneName string

@description('The name of the CNAME record to create within the DNS zone. The record will be an alias to your Front Door endpoint.')
param cnameRecordName string = 'www'

var profileName = endpointName

// Create a valid resource name for the custom domain. Resource names don't include periods.
var customDomainResourceName = replace('${cnameRecordName}.${dnsZoneName}', '.', '-')
var dnsRecordTimeToLive = 3600

resource dnsZone 'Microsoft.Network/dnsZones@2023-07-01-preview' existing = {
  name: dnsZoneName

  resource cnameRecord 'CNAME' = {
    name: cnameRecordName
    properties: {
      TTL: dnsRecordTimeToLive
      CNAMERecord: {
        cname: profile::endpoint.properties.hostName
      }
    }
  }

  resource validationTxtRecord 'TXT' = {
    name: '_dnsauth.${cnameRecordName}'
    properties: {
      TTL: dnsRecordTimeToLive
      TXTRecords: [
        {
          value: [
            profile::customDomain.properties.validationProperties.validationToken
          ]
        }
      ]
    }
  }
}

resource profile 'Microsoft.Cdn/profiles@2025-06-01' = {
  name: profileName
  tags: tags
  location: 'global'
  sku: {
    name: skuName
  }

  resource endpoint 'afdEndpoints' = {
    name: endpointName
    tags: tags
    location: 'global'
    properties: {
      enabledState: 'Enabled'
    }

    resource uiRoute 'routes' = {
      name: 'ui-route'
      dependsOn: [
        uiOriginGroup::uiOrigin // This explicit dependency is required to ensure that the origin group is not empty when the route is created.
      ]
      properties: {
        customDomains: [
          {
            id: customDomain.id
          }
        ]
        originGroup: {
          id: uiOriginGroup.id
        }
        supportedProtocols: [
          'Http'
          'Https'
        ]
        patternsToMatch: [
          '/*'
        ]
        forwardingProtocol: 'HttpsOnly'
        linkToDefaultDomain: 'Enabled'
        httpsRedirect: 'Enabled'
      }
    }

    resource apiRoute 'routes' = {
      name: 'api-route'
      dependsOn: [
        apiOriginGroup::apiOrigin // This explicit dependency is required to ensure that the origin group is not empty when the route is created.
      ]
      properties: {
        customDomains: [
          {
            id: customDomain.id
          }
        ]
        originGroup: {
          id: apiOriginGroup.id
        }
        supportedProtocols: [
          'Http'
          'Https'
        ]
        patternsToMatch: [
          '/api/*'
          '/health/*'
          '/api-docs'
          '/openapi/*'
        ]
        forwardingProtocol: 'HttpsOnly'
        linkToDefaultDomain: 'Enabled'
        httpsRedirect: 'Enabled'
      }
    }
  }

  resource uiOriginGroup 'originGroups' = {
    name: 'ui-origin-group'
    properties: {
      loadBalancingSettings: {
        sampleSize: 4
        successfulSamplesRequired: 3
      }
      // healthProbeSettings: {
      //   probePath: '/'
      //   probeRequestType: 'HEAD'
      //   probeProtocol: 'Http' // The UI needs http for some reason
      //   probeIntervalInSeconds: 100
      // }
    }

    resource uiOrigin 'origins' = {
      name: 'ui-origin'
      properties: {
        hostName: uiOriginHostName
        httpPort: 80
        httpsPort: 443
        originHostHeader: uiOriginHostName
        priority: 1
        weight: 1000
      }
    }
  }

  resource apiOriginGroup 'originGroups' = {
    name: 'api-origin-group'
    properties: {
      loadBalancingSettings: {
        sampleSize: 4
        successfulSamplesRequired: 3
      }
      // healthProbeSettings: {
      //   probePath: '/health/live'
      //   probeRequestType: 'GET'
      //   probeProtocol: 'Https'
      //   probeIntervalInSeconds: 100
      // }
    }

    resource apiOrigin 'origins' = {
      name: 'api-origin'
      properties: {
        hostName: apiOriginHostName
        httpPort: 80
        httpsPort: 443
        originHostHeader: apiOriginHostName
        priority: 1
        weight: 1000
      }
    }
  }

  resource customDomain 'customDomains' = {
    name: customDomainResourceName
    properties: {
      hostName: substring(dnsZone::cnameRecord.properties.fqdn, 0, length(dnsZone::cnameRecord.properties.fqdn) - 1)
      tlsSettings: {
        certificateType: 'ManagedCertificate'
        minimumTlsVersion: 'TLS12'
      }
    }
  }
}
