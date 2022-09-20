param applicationInsightsName string
param functionAppName string
param functionPlanName string
param StorageAccount_Name string
param defaultLocation string = resourceGroup().location

resource functionPlanResource 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: functionPlanName
  location: defaultLocation
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
  }
  kind: 'functionapp'
  properties: {
    reserved: true
  }
}

resource functionAppResource 'Microsoft.Web/sites@2021-01-15' = {
  name: functionAppName
  location: defaultLocation
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: functionPlanResource.id
    reserved: true
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference(resourceId('microsoft.insights/components/', applicationInsightsName), '2015-05-01').InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${StorageAccount_Name};AccountKey=${listKeys(resourceId('Microsoft.Storage/storageAccounts', StorageAccount_Name), '2015-05-01-preview').key1}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
    }
  }
}
