using './main.bicep'

param environment = 'dev'
param location = 'South Africa North'
param applicationName = 'fake-survey-generator'
param dnsZoneName = 'mysecondarydomain.com'
param routeCustomDomainSubdomain = 'fakesurveygeneratortest'
param routeCustomDomainName = 'fakesurveygeneratortest.mysecondarydomain.com'
