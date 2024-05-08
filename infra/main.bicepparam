using './main.bicep'

param environment = 'dev'
param location = 'South Africa North'
param applicationName = 'fake-survey-generator'
param dnsZoneName = 'mysecondarydomain.com'
param sqlAzureAdAdministratorLogin = 'SQL Server Administrators'
param sqlAzureAdAdministratorObjectId = '7accb81c-4513-4df6-9eb7-791ac78e8fdb'
param apiAppExists = bool(readEnvironmentVariable('SERVICE_API_RESOURCE_EXISTS', 'false'))
param uiAppExists = bool(readEnvironmentVariable('SERVICE_UI_RESOURCE_EXISTS', 'false'))
