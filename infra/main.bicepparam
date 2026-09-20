using './main.bicep'

param environment = 'dev'
param location = 'South Africa North'
param applicationName = 'fake-survey-generator'
param dnsZoneName = 'mysecondarydomain.com'
param typeSafeApiKey = readEnvironmentVariable('TYPE_SAFE_API_KEY', '')
