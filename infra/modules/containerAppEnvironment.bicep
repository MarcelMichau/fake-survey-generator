@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('Specifies the name of the Container App Environment')
param containerAppEnvName string

@description('Specifies the name of the log analytics workspace')
param logAnalyticsName string

@description('Subnet Resource ID for the infrastructure subnet')
param subnetResourceId string

@description('Specifies the name of the HTTP route configuration')
param routeConfigName string

@description('Specifies the custom domain hostname for route-based routing')
param routeCustomDomainName string

@description('Specifies the route custom domain binding type')
@allowed([
  'Auto'
  'Disabled'
  'SniEnabled'
])
param routeCustomDomainBindingType string = 'Auto'

@description('Specifies the managed certificate resource name')
param managedCertificateName string

@description('Specifies the domain validation method for managed certificate creation')
@allowed([
  'CNAME'
  'HTTP'
  'TXT'
])
param managedCertificateDomainValidation string = 'TXT'

@description('Specifies the target API container app name for route rules')
param apiContainerAppName string

@description('Specifies the target UI container app name for route rules')
param uiContainerAppName string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-07-01' existing = {
  name: logAnalyticsName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: containerAppEnvName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: subnetResourceId
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }

  resource managedCertificate 'managedCertificates' = {
    name: managedCertificateName
    location: location
    tags: tags
    properties: {
      subjectName: routeCustomDomainName
      domainControlValidation: managedCertificateDomainValidation
    }
    dependsOn: [
      httpRouteConfig
    ]
  }

  resource httpRouteConfig 'httpRouteConfigs' = {
    name: routeConfigName
    properties: {
      customDomains: [
        {
          name: routeCustomDomainName
          bindingType: routeCustomDomainBindingType
        }
      ]
      rules: [
        {
          description: 'API Rule'
          routes: [
            {
              match: {
                prefix: '/api'
              }
            }
            {
              match: {
                prefix: '/api-docs'
              }
              action: {
                prefixRewrite: '/'
              }
            }
            {
              match: {
                prefix: '/openapi'
              }
              action: {
                prefixRewrite: '/'
              }
            }
          ]
          targets: [
            {
              containerApp: apiContainerAppName
            }
          ]
        }
        {
          description: 'UI Rule'
          routes: [
            {
              match: {
                prefix: '/'
              }
            }
          ]
          targets: [
            {
              containerApp: uiContainerAppName
            }
          ]
        }
      ]
    }
  }
}

output containerAppEnvironmentId string = containerAppEnvironment.id
output containerAppEnvironmentName string = containerAppEnvironment.name
output fqdn string = containerAppEnvironment.properties.defaultDomain
output defaultDomain string = containerAppEnvironment.properties.defaultDomain
output staticIp string = containerAppEnvironment.properties.staticIp
output domainVerificationId string = containerAppEnvironment.properties.customDomainConfiguration.customDomainVerificationId
