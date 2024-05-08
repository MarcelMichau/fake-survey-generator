param containerAppEnvironmentName string
param componentName string
param componentType string
param componentVersion string = 'v1'
param metadata array
param scopes array

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-02-preview' existing = {
  name: containerAppEnvironmentName
}

resource daprSecretStoreComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-02-preview' = {
  name: componentName
  parent: containerAppEnvironment
  properties: {
    componentType: componentType
    version: componentVersion
    metadata: metadata
    scopes: scopes
  }
}
