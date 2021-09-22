@description('Name of the Application Insights resource')
param name string

@description('Location of the Application Insights resource')
param location string = resourceGroup().location

resource applicationInsights 'Microsoft.Insights/components@2018-05-01-preview' = {
  name: name
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
  }
}

output instrumentationKey string = reference(applicationInsights.id, '2018-05-01-preview').InstrumentationKey
