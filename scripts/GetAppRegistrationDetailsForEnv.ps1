# API App Registration
$ApiAppRegistrationName = 'insert-api-app-registration-here'
$AppRegistration = az ad app list --display-name $ApiAppRegistrationName | ConvertFrom-Json

if ($AppRegistration.Count -eq 0) {
  Write-Host "No app registration found with display name $ApiAppRegistrationName"
  exit 1
}

$AppRegistrationClientId = $AppRegistration[0].appId
azd env set API_APP_REGISTRATION_CLIENT_ID $AppRegistrationClientId
Write-Host "Set environment variable API_APP_REGISTRATION_CLIENT_ID to $AppRegistrationClientId"

$ExistingEnvVariables = azd env get-values --output json | ConvertFrom-Json

if ($ExistingEnvVariables.API_APP_REGISTRATION_CLIENT_SECRET || $env:API_APP_REGISTRATION_CLIENT_SECRET) {
  Write-Host "API_APP_REGISTRATION_CLIENT_SECRET already exists in the environment variables. Skipping creation of new password credential"
}
else {
  $CurrentUser = az account show | ConvertFrom-Json
  $CurrentUser = $CurrentUser.user.name
  $CurrentUser = $CurrentUser.Replace("@", "-")
  $CurrentUser = $CurrentUser.Replace(".", "-").ToLower()
    
  Write-Host "Creating a new password credential for $($ApiAppRegistrationName) with display name local-dev-cert-$CurrentUser"

  try {
    $PasswordCredential = az ad app credential reset --append --id $AppRegistration[0].appId --display-name "local-dev-cert-$CurrentUser" --end-date $(Get-Date).AddDays(7) | ConvertFrom-Json
  }
  catch {
    Write-Host "Failed to create a new password credential for $($ApiAppRegistrationName) - You will need to be an owner on the App Registration in order to create a new password credential"
    exit 1
  }
    
  azd env set API_APP_REGISTRATION_CLIENT_SECRET $PasswordCredential.password
  Write-Host "Set environment variable API_APP_REGISTRATION_CLIENT_SECRET"
}

# Client App Registration - No need to create a secret as the client app is a public client in Entra ID
$ClientAppRegistrationName = 'insert-ui-app-registration-here'

$AppRegistration = az ad app list --display-name $ClientAppRegistrationName | ConvertFrom-Json

if ($AppRegistration.Count -eq 0) {
  Write-Host "No app registration found with display name $ClientAppRegistrationName"
  exit 1
}

$AppRegistrationClientId = $AppRegistration[0].appId
azd env set CLIENT_APP_REGISTRATION_CLIENT_ID $AppRegistrationClientId
Write-Host "Set environment variable CLIENT_APP_REGISTRATION_CLIENT_ID to $AppRegistrationClientId"