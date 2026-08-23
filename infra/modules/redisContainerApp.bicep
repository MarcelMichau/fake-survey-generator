@description('Tags to apply to the resource')
param tags object

@description('Name of the Redis Container App')
param name string

@description('Container Apps environment resource ID')
param containerAppEnvironmentId string

@description('Password required by clients connecting to Redis')
@secure()
param redisPassword string

resource redisContainerApp 'Microsoft.App/containerApps@2026-01-01' = {
  name: name
  location: resourceGroup().location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      secrets: [
        {
          name: 'redis-password'
          value: redisPassword
        }
      ]
      ingress: {
        external: false
        targetPort: 6379
        transport: 'tcp'
      }
    }
    template: {
      revisionSuffix: 'redis-8-8-2'
      containers: [
        {
          name: 'redis'
          image: 'redis:8.8.2-alpine'
          command: [
            '/bin/sh'
          ]
          args: [
            '-c'
            'exec redis-server --appendonly no --requirepass "$REDIS_PASSWORD"'
          ]
          env: [
            {
              name: 'REDIS_PASSWORD'
              secretRef: 'redis-password'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
    workloadProfileName: 'Consumption'
  }
}

output hostName string = redisContainerApp.properties.configuration.ingress.fqdn
