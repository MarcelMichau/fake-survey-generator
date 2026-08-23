@description('Location')
param location string = resourceGroup().location

@description('Tags to apply to the generated managed identity')
param tags object

@description('Name of the user-assigned managed identity used by the deployment script')
param name string

@description('Name of the Key Vault that stores the Redis password')
param keyVaultName string

var keyVaultSecretsOfficer = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
)

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-05-31-preview' = {
  name: name
  location: location
  tags: tags
}

resource keyVaultSecretsOfficerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, managedIdentity.id, keyVaultSecretsOfficer)
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource redisPassword 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'generate-redis-password'
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    azCliVersion: '2.67.0'
    timeout: 'PT10M'
    retentionInterval: 'PT1H'
    cleanupPreference: 'OnSuccess'
    arguments: '${keyVault.properties.vaultUri} RedisPassword'
    scriptContent: '''
      #!/usr/bin/env bash
      set -euo pipefail

      vault_uri="$1"
      secret_name="$2"
      vault_name="${vault_uri#https://}"
      vault_name="${vault_name%%.*}"

      secret_id="$(az keyvault secret show --id "${vault_uri}secrets/${secret_name}" --query id --output tsv 2>/dev/null || true)"
      if [ -z "$secret_id" ]; then
        password="$(openssl rand -hex 32)"
        secret_id="$(az keyvault secret set --vault-name "$vault_name" --name "$secret_name" --value "$password" --query id --output tsv)"
      fi

      jq -n --arg secretUrl "${secret_id%/*}" '{secretUrl: $secretUrl}' > "$AZ_SCRIPTS_OUTPUT_PATH"
    '''
  }
  dependsOn: [
    keyVaultSecretsOfficerRoleAssignment
  ]
}

output secretUrl string = redisPassword.properties.outputs.secretUrl