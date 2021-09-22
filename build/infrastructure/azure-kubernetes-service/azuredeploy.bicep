@description('The name of the Managed Cluster resource.')
param clusterName string = 'aks-cluster'

@description('Name of the resource group containing agent pool nodes.')
param nodeResourceGroupName string = 'aks-cluster-infrastructure'

@description('The location of the Managed Cluster resource.')
param location string = resourceGroup().location

@description('Version of Kubernetes specified when creating the managed cluster.')
param kubernetesVersion string = '1.21.2'

@description('Optional DNS prefix to use with hosted Kubernetes API server FQDN.')
param dnsPrefix string = 'aks-cluster-dns'

@description('Disk size (in GB) to provision for each of the agent pool nodes. This value ranges from 0 to 1023. Specifying 0 will apply the default disk size for that agentVMSize.')
@minValue(0)
@maxValue(1023)
param osDiskSizeGB int = 0

@description('The number of nodes for the cluster.')
@minValue(1)
@maxValue(1)
param agentCount int = 1

@description('The size of the Virtual Machine.')
param agentVMSize string = 'Standard_B2s'

@description('Network plugin used for building Kubernetes network.')
@allowed([
  'azure'
  'kubenet'
])
param networkPlugin string = 'kubenet'

@description('Boolean flag to turn on and off of RBAC.')
param enableRBAC bool = true

@description('The type of operating system.')
@allowed([
  'Linux'
])
param osType string = 'Linux'

@description('Enable private network access to the Kubernetes cluster.')
param enablePrivateCluster bool = false

@description('Boolean flag to turn on and off http application routing.')
param enableHttpApplicationRouting bool = false

resource aksCluster 'Microsoft.ContainerService/managedClusters@2021-05-01' = {
  location: location
  name: clusterName
  properties: {
    kubernetesVersion: kubernetesVersion
    enableRBAC: enableRBAC
    dnsPrefix: dnsPrefix
    agentPoolProfiles: [
      {
        name: 'agentpool'
        osDiskSizeGB: osDiskSizeGB
        count: agentCount
        vmSize: agentVMSize
        osType: osType
        type: 'VirtualMachineScaleSets'
        mode: 'System'
      }
    ]
    nodeResourceGroup: nodeResourceGroupName
    networkProfile: {
      loadBalancerSku: 'standard'
      networkPlugin: networkPlugin
    }
    apiServerAccessProfile: {
      enablePrivateCluster: enablePrivateCluster
    }
    addonProfiles: {
      httpApplicationRouting: {
        enabled: enableHttpApplicationRouting
      }
    }
  }
  tags: {}
  identity: {
    type: 'SystemAssigned'
  }
}

output controlPlaneFQDN string = aksCluster.properties.fqdn
