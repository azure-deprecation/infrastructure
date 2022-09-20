param ApplicationInsights_Name string
param Connections_CosmosDb_Name string
param Connections_ServiceBus_Name string
param CosmosDb_Account_Name string
param Function_App_Name string
param Function_Plan_Name string
param GitHub_Owner string
param GitHub_Repo_Name string

@secure()
param GitHub_Token string
param ServiceBus_Namespace_Name string
param StorageAccount_Name string
param Workflow_Name string

param defaultLocation string = resourceGroup().location
var cosmosDbConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/documentdb'
var serviceBusConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/servicebus'

resource Function_Plan_Name_resource 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: Function_Plan_Name
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

resource Function_App_Name_resource 'Microsoft.Web/sites@2021-01-15' = {
  name: Function_App_Name
  location: defaultLocation
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: Function_Plan_Name_resource.id
    reserved: true
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference(resourceId('microsoft.insights/components/', ApplicationInsights_Name), '2015-05-01').InstrumentationKey
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
        {
          name: 'GitHub_Owner'
          value: GitHub_Owner
        }
        {
          name: 'GitHub_RepoName'
          value: GitHub_Repo_Name
        }
        {
          name: 'GitHub_Token'
          value: GitHub_Token
        }
        {
          name: 'ServiceBus_ConnectionString'
          value: listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', ServiceBus_Namespace_Name, 'RootManageSharedAccessKey'), '2017-04-01').primaryConnectionString
        }
      ]
    }
  }
}

resource Connections_ServiceBus_Name_resource 'Microsoft.Web/connections@2016-06-01' = {
  name: Connections_ServiceBus_Name
  location: defaultLocation
  properties: {
    displayName: Connections_ServiceBus_Name
    customParameterValues: {
    }
    api: {
      id: serviceBusConnectionId
    }
    parameterValues: {
      connectionString: listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', ServiceBus_Namespace_Name, 'RootManageSharedAccessKey'), '2017-04-01').primaryConnectionString
    }
  }
  dependsOn: []
}

resource Connections_CosmosDb_Name_resource 'Microsoft.Web/connections@2016-06-01' = {
  name: Connections_CosmosDb_Name
  location: defaultLocation
  properties: {
    displayName: Connections_CosmosDb_Name
    customParameterValues: {
    }
    api: {
      id: cosmosDbConnectionId
    }
    parameterValues: {
      databaseAccount: CosmosDb_Account_Name
      accessKey: listKeys(resourceId('Microsoft.DocumentDB/databaseAccounts', CosmosDb_Account_Name), '2015-04-08').primaryMasterKey
    }
  }
  dependsOn: []
}

resource Workflow_Name_resource 'Microsoft.Logic/workflows@2017-07-01' = {
  name: Workflow_Name
  location: defaultLocation
  properties: {
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          defaultValue: {
          }
          type: 'Object'
        }
      }
      triggers: {
        'When_a_deprecation_notice_is_published_(peek-lock)': {
          recurrence: {
            frequency: 'Hour'
            interval: 1
          }
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'servicebus\'][\'connectionId\']'
              }
            }
            method: 'get'
            path: '/@{encodeURIComponent(encodeURIComponent(\'new-deprecation-notices\'))}/subscriptions/@{encodeURIComponent(\'persist-new-notice\')}/messages/head/peek'
            queries: {
              sessionId: 'None'
              subscriptionType: 'Main'
            }
          }
        }
      }
      actions: {
        Complete_the_message: {
          runAfter: {
            Create_or_update_new_deprecation_document: [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'servicebus\'][\'connectionId\']'
              }
            }
            method: 'delete'
            path: '/@{encodeURIComponent(encodeURIComponent(\'new-deprecation-notices\'))}/subscriptions/@{encodeURIComponent(\'persist-new-notice\')}/messages/complete'
            queries: {
              lockToken: '@triggerBody()?[\'LockToken\']'
              sessionId: ''
              subscriptionType: 'Main'
            }
          }
        }
        Create_or_update_new_deprecation_document: {
          runAfter: {
          }
          type: 'ApiConnection'
          inputs: {
            body: '@addProperty(json(decodeBase64(triggerBody()?[\'ContentData\'])), \'id\', guid())'
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'documentdb\'][\'connectionId\']'
              }
            }
            method: 'post'
            path: '/v2/cosmosdb/@{encodeURIComponent(\'AccountNameFromSettings\')}/dbs/@{encodeURIComponent(\'deprecation-db\')}/colls/@{encodeURIComponent(\'deprecations\')}/docs'
          }
        }
      }
      outputs: {
      }
    }
    parameters: {
      '$connections': {
        value: {
          documentdb: {
            connectionId: Connections_CosmosDb_Name_resource.id
            connectionName: Connections_CosmosDb_Name
            id: cosmosDbConnectionId
          }
          servicebus: {
            connectionId: Connections_ServiceBus_Name_resource.id
            connectionName: Connections_ServiceBus_Name
            id: serviceBusConnectionId
          }
        }
      }
    }
  }
}
