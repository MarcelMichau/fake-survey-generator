using './main.bicep'

param environment = 'dev'
param location = 'South Africa North'
param applicationName = 'fake-survey-generator'
param dnsZoneName = 'mysecondarydomain.com'
// Microsoft-Decision-1 is not offered to new deployments in South Africa North; France Central serves the EU data zone.
param foundryLocation = 'francecentral'
// Microsoft Entra object IDs granted Cognitive Services User on Foundry for local development.
param foundryDeveloperPrincipalIds = [
  '8faa7d79-19bf-4b73-a9b1-dbaf5efc3fcf' // Marcel Michau
]
