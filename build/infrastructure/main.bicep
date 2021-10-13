targetScope = 'subscription'

param location string = 'South Africa North'
param resourceGroupName string = 'rg-fake-survey-generator-new'
param applicationInsightsName string = 'appi-fake-survey-generator'
param dnsZoneName string = 'mysecondarydomain.com'
param keyVaultName string = 'kv-fake-survey-generator'
param containerRegistryName string = 'acrfakesurveygenerator'
param redisCacheName string = 'redis-fake-survey-generator'
param sqlServerName string = 'sql-marcel-michau'
param sqlDatabaseName string = 'sqldb-fake-survey-generator'
param sqlAdministratorLogin string
param sqlAdministratorLoginPassword string
param sqlAzureAdAdministratorLogin string
param sqlAzureAdAdministratorObjectId string
param externalDnsIdentityName string = 'mi-aks-external-dns'
param certManagerIdentityName string = 'mi-aks-cert-manager'
param kubernetesClusterName string = 'aks-cluster'
param kubernetesNodeResourceGroupName string = 'rg-aks-cluster-infrastructure-new'

resource fakeSurveyGeneratorResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

module applicationInsights 'modules/azure-application-insights/azuredeploy.bicep' = {
  name: 'applicationInsights'
  params: {
    location: location
    name: applicationInsightsName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module dnsZone 'modules/dns-zone/azuredeploy.bicep' = {
  name: 'dnsZone'
  params: {
    name: dnsZoneName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module keyVault 'modules/azure-key-vault/azuredeploy.bicep' = {
  name: 'keyVault'
  params: {
    location: location
    name: keyVaultName
    secretsObject: {
      secrets: [
        {
          secretName: 'HealthCheckSecret'
          secretValue: 'healthy'
        }
      ]
    }
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module containerRegistry 'modules/azure-container-registry/azuredeploy.bicep' = {
  name: 'containerRegistry'
  params: {
    name: containerRegistryName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module redisCache 'modules/azure-redis-cache/azuredeploy.bicep' = {
  name: 'redisCache'
  params: {
    name: redisCacheName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module azureSql 'modules/azure-sql/azuredeploy.bicep' = {
  name: 'azureSql'
  params: {
    serverName: sqlServerName
    databaseName: sqlDatabaseName
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorLoginPassword
    azureAdAdministratorLogin: sqlAzureAdAdministratorLogin
    azureAdAdministratorObjectId: sqlAzureAdAdministratorObjectId
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module aksManagedIdentities 'modules/aks-managed-identities/azuredeploy.bicep' = {
  name: 'aksManagedIdentities'
  params: {
    dnsZoneName: dnsZoneName
    externalDnsIdentityName: externalDnsIdentityName
    certManagerIdentityName: certManagerIdentityName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

module aksCluster 'modules/azure-kubernetes-service/azuredeploy.bicep' = {
  name: 'aksCluster'
  params: {
    clusterName: kubernetesClusterName
    nodeResourceGroupName: kubernetesNodeResourceGroupName
  }
  scope: fakeSurveyGeneratorResourceGroup
}

resource aksNodeResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: kubernetesNodeResourceGroupName
}

module nodeResourceGroupManagedIdentityRoleAssignments 'modules/aks-managed-identity-role-assignments/node-resource-group/azuredeploy.bicep' = {
  name: 'nodeResourceGroupManagedIdentityRoleAssignments'
  params: {
    aksClusterManagedIdentityName: '${kubernetesClusterName}-agentpool'
  }
  scope: aksNodeResourceGroup
}

module clusterResourceGroupManagedIdentityRoleAssignments 'modules/aks-managed-identity-role-assignments/cluster-resource-group/azuredeploy.bicep' = {
  name: 'clusterResourceGroupManagedIdentityRoleAssignments'
  params: {
    aksClusterManagedIdentityName: '${kubernetesClusterName}-agentpool'
    aksClusterNodeResourceGroupName: kubernetesNodeResourceGroupName
    azureContainerRegistryName: containerRegistryName
  }
  scope: fakeSurveyGeneratorResourceGroup
}
