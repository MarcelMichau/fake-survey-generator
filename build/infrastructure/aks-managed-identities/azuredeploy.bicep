@description('The location of the Managed Identities')
param location string = resourceGroup().location

@description('User Assigned External DNS Identity Name')
param externalDnsIdentityName string

@description('DNS Zone for which the externalDnsIdentity needs to have Contributor Role access')
param dnsZoneName string

@description('User Assigned Cert Manager Identity Name')
param certManagerIdentityName string

var readerRole = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/acdd72a7-3385-48ef-bd42-f606fba81ae7'
var dnsZoneContributorRole = '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/befefa01-2a29-4197-83a8-272ff33ce314'

resource externalDnsIdentityName_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: externalDnsIdentityName
  location: location
}

resource certManagerIdentityName_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: certManagerIdentityName
  location: location
}

resource id_externalDnsIdentityName_Reader 'Microsoft.Authorization/roleAssignments@2017-09-01' = {
  name: guid('${resourceGroup().id}${externalDnsIdentityName}Reader')
  properties: {
    roleDefinitionId: readerRole
    principalId: reference(externalDnsIdentityName_resource.id, '2018-11-30').principalId
    scope: resourceGroup().id
    principalType: 'ServicePrincipal'
  }
}

resource dnsZoneName_Microsoft_Authorization_externalDnsIdentityName_dnsZoneContributorRole 'Microsoft.Network/dnsZones/providers/roleAssignments@2018-09-01-preview' = {
  name: '${dnsZoneName}/Microsoft.Authorization/${guid(uniqueString(concat(externalDnsIdentityName, dnsZoneContributorRole)))}'
  properties: {
    roleDefinitionId: dnsZoneContributorRole
    principalId: reference(externalDnsIdentityName_resource.id, '2018-11-30').principalId
    scope: resourceId('Microsoft.Network/dnsZones', dnsZoneName)
    principalType: 'ServicePrincipal'
  }
}

resource dnsZoneName_Microsoft_Authorization_certManagerIdentityName_dnsZoneContributorRole 'Microsoft.Network/dnsZones/providers/roleAssignments@2018-09-01-preview' = {
  name: '${dnsZoneName}/Microsoft.Authorization/${guid(uniqueString(concat(certManagerIdentityName, dnsZoneContributorRole)))}'
  properties: {
    roleDefinitionId: dnsZoneContributorRole
    principalId: reference(certManagerIdentityName_resource.id, '2018-11-30').principalId
    scope: resourceId('Microsoft.Network/dnsZones', dnsZoneName)
    principalType: 'ServicePrincipal'
  }
}

output externalDnsIdentityName string = externalDnsIdentityName
output externalDnsIdentityResourceId string = externalDnsIdentityName_resource.id
output externalDnsIdentityClientId string = reference(externalDnsIdentityName_resource.id, '2018-11-30', 'Full').properties.clientId
output certManagerIdentityName string = certManagerIdentityName
output certManagerIdentityResourceId string = certManagerIdentityName_resource.id
output certManagerIdentityClientId string = reference(certManagerIdentityName_resource.id, '2018-11-30', 'Full').properties.clientId
