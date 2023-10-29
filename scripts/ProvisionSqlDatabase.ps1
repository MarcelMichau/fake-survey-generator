Import-Module SQLServer
Import-Module Az.Accounts -MinimumVersion 2.2.0
Import-Module Az.Sql

# Make sure that the user executing this script is in the Entra ID Admins group for the Azure SQL Server

$ExistingEnvVariables = azd env get-values --output json | ConvertFrom-Json

Connect-AzAccount
Set-AzContext -Subscription $ExistingEnvVariables.AZURE_SUBSCRIPTION_ID
$access_token = (Get-AzAccessToken -ResourceUrl https://database.windows.net).Token

$ResourceGroupName = $ExistingEnvVariables.AZURE_RESOURCE_GROUP_NAME
$SqlServerName = $ExistingEnvVariables.SQL_SERVER_NAME
$SqlServerInstance = $ExistingEnvVariables.SQL_SERVER_INSTANCE
$DatabaseName = $ExistingEnvVariables.SQL_DATABASE_NAME
$ManagedIdentityName = $ExistingEnvVariables.SERVICE_API_IDENTITY_NAME

$localIP = (New-Object net.webclient).downloadstring("https://api.ipify.org")

New-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -FirewallRuleName "Allow local IP" -StartIpAddress $localIP -EndIpAddress $localIP

try {
  Invoke-Sqlcmd -InputFile "scripts\CreateManagedIdentitySqlUser.sql" -Variable "ManagedIdentityName=$ManagedIdentityName" -ServerInstance $SqlServerInstance -Database $DatabaseName -AccessToken $access_token
  Write-Output "Created managed identity user $ManagedIdentityName"

  Invoke-Sqlcmd -InputFile "src\server\FakeSurveyGenerator.Application\DbMigrationScript.sql" -ServerInstance $SqlServerInstance -Database $DatabaseName -AccessToken $access_token
  Write-Output "Created database structure for $DatabaseName"
}
finally {
  Remove-AzSqlServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $SqlServerName -FirewallRuleName "Allow local IP"
}