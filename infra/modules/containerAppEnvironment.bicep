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

  // resource managedCertificate 'managedCertificates' = {
  //   name: 'fake-survey-generator-cert'
  //   location: location
  //   tags: tags
  //   properties: {
  //     subjectName: 'fakesurveygeneratortest.mysecondarydomain.com'
  //     domainControlValidation: 'TXT'
  //   }
  //   dependsOn: [
  //     httpRouteConfig
  //   ]
  // }

  // resource httpRouteConfig 'httpRouteConfigs' = {
  //   name: 'fakesurveygenerator'
  //   properties: {
  //     customDomains: [
  //       {
  //         name: 'fakesurveygeneratortest.mysecondarydomain.com'
  //         bindingType: 'Auto'
  //       }
  //     ]
  //     rules: [
  //       {
  //         description: 'API Rule'
  //         routes: [
  //           {
  //             match: {
  //               prefix: '/api'
  //             }
  //           }
  //           {
  //             match: {
  //               prefix: '/api-docs'
  //             }
  //             action: {
  //               prefixRewrite: '/'
  //             }
  //           }
  //           {
  //             match: {
  //               prefix: '/openapi'
  //             }
  //             action: {
  //               prefixRewrite: '/'
  //             }
  //           }
  //         ]
  //         targets: [
  //           {
  //             containerApp: 'ca-fake-survey-generator-api'
  //           }
  //         ]
  //       }
  //       {
  //         description: 'UI Rule'
  //         routes: [
  //           {
  //             match: {
  //               prefix: '/'
  //             }
  //           }
  //         ]
  //         targets: [
  //           {
  //             containerApp: 'ca-fake-survey-generator-ui'
  //           }
  //         ]
  //       }
  //     ]
  //   }
  // }
}

output containerAppEnvironmentId string = containerAppEnvironment.id
output containerAppEnvironmentName string = containerAppEnvironment.name
output fqdn string = containerAppEnvironment.properties.defaultDomain
output defaultDomain string = containerAppEnvironment.properties.defaultDomain
