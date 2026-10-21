@description('Location')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object

@description('The name of the Microsoft Foundry resource. Also used as its custom subdomain.')
param name string

@description('The name of the Microsoft Foundry project')
param projectName string

@description('The name of the Microsoft-Decision-1 model deployment')
param decisionDeploymentName string = 'decision-1'

@description('Requests per minute available to the Microsoft-Decision-1 deployment')
param decisionDeploymentCapacity int = 100

@description('Principal ID of the API managed identity that calls the model')
param apiPrincipalId string

@description('Principal IDs of developers who call the model from local development')
param developerPrincipalIds array = []

var cognitiveServicesUser = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'a97b65f3-24c7-4388-baec-2e87135dc908'
)

resource foundry 'Microsoft.CognitiveServices/accounts@2026-07-01' = {
  name: name
  location: location
  tags: tags
  kind: 'AIServices'
  sku: {
    name: 'S0'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: name
    allowProjectManagement: true
    // Microsoft Entra ID only: no API keys.
    disableLocalAuth: true
    publicNetworkAccess: 'Enabled'
  }

  resource project 'projects@2026-07-01' = {
    name: projectName
    location: location
    tags: tags
    identity: {
      type: 'SystemAssigned'
    }
    properties: {}
  }

  resource decisionDeployment 'deployments@2026-07-01' = {
    name: decisionDeploymentName
    sku: {
      // While Microsoft-Decision-1 is in preview, new deployments are only available as DataZoneStandard.
      name: 'DataZoneStandard'
      capacity: decisionDeploymentCapacity
    }
    properties: {
      model: {
        format: 'Microsoft'
        name: 'Microsoft-Decision-1'
        version: '1'
      }
      raiPolicyName: 'Microsoft.DefaultV2'
    }
    // Foundry rejects concurrent changes to an account's child resources.
    dependsOn: [
      project
    ]
  }
}

resource apiCognitiveServicesUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundry.id, apiPrincipalId, cognitiveServicesUser)
  scope: foundry
  properties: {
    roleDefinitionId: cognitiveServicesUser
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource developerCognitiveServicesUserRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [
  for developerPrincipalId in developerPrincipalIds: {
    name: guid(foundry.id, developerPrincipalId, cognitiveServicesUser)
    scope: foundry
    properties: {
      roleDefinitionId: cognitiveServicesUser
      principalId: developerPrincipalId
      principalType: 'User'
    }
  }
]

output name string = foundry.name
output endpoint string = 'https://${foundry.properties.customSubDomainName}.services.ai.azure.com/'
output projectName string = foundry::project.name
output decisionDeploymentName string = foundry::decisionDeployment.name
