using './main.bicep'

param environment = 'dev'
param location = 'South Africa North'
param applicationName = 'fsg-azd-test'
param dnsZoneName = 'mytertiarydomain.com'
param sqlAzureAdAdministratorLogin = 'azure-sql-administrators'
param sqlAzureAdAdministratorObjectId = '6fe16a87-222a-4a1a-b820-9b7fd37b44a8'
param apiAppExists = bool(readEnvironmentVariable('SERVICE_API_RESOURCE_EXISTS', 'false'))
param uiAppExists = bool(readEnvironmentVariable('SERVICE_UI_RESOURCE_EXISTS', 'false'))
