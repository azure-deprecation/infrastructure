param applicationInsightsName string
param cosmosDbConnectionName string
param serviceBusConnectionName string
param twitterConnectionName string
param cosmosDbAccountName string
param functionAppName string
param functionPlanName string
param eventGridTopicName string

param serviceBusNamespaceName string
param storageAccountName string
param tweetNewDeprecationWorkflowName string

param defaultLocation string = resourceGroup().location
var cosmosDbConnectionId = subscriptionResourceId('Microsoft.Web/locations/managedApis', defaultLocation, 'documentdb')
var serviceBusConnectionId = subscriptionResourceId('Microsoft.Web/locations/managedApis', defaultLocation, 'servicebus')
var twitterConnectionId = subscriptionResourceId('Microsoft.Web/locations/managedApis', defaultLocation, 'twitter')

resource functionAppPlan 'Microsoft.Web/serverfarms@2021-01-15' = {
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

resource functionApp 'Microsoft.Web/sites@2021-01-15' = {
  name: functionAppName
  location: defaultLocation
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: functionAppPlan.id
    reserved: true
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: reference(resourceId('microsoft.insights/components/', applicationInsightsName), '2015-05-01').InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys(resourceId('Microsoft.Storage/storageAccounts', storageAccountName), '2015-05-01-preview').key1}'
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
          name: 'EVENTGRID_AUTH_KEY'
          value: listKeys(resourceId('Microsoft.EventGrid/topics', eventGridTopicName), '2020-06-01').key1
        }
        {
          name: 'EVENTGRID_ENDPOINT'
          value: 'https://${eventGridTopicName}.${defaultLocation}-1.eventgrid.azure.net/api/events'
        }
        {
          name: 'ServiceBus_ConnectionString'
          value: listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', serviceBusNamespaceName, 'RootManageSharedAccessKey'), '2017-04-01').primaryConnectionString
        }
      ]
    }
  }
}

resource serviceBusConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: serviceBusConnectionName
  location: defaultLocation
  properties: {
    displayName: serviceBusConnectionName
    customParameterValues: {
    }
    api: {
      id: serviceBusConnectionId
    }
    parameterValues: {
      connectionString: listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', serviceBusNamespaceName, 'RootManageSharedAccessKey'), '2017-04-01').primaryConnectionString
    }
  }
  dependsOn: []
}

resource twitterConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: twitterConnectionName
  location: defaultLocation
  properties: {
    displayName: twitterConnectionName
    customParameterValues: {
    }
    api: {
      id: twitterConnectionId
    }
    parameterValues: {
    }
  }
  dependsOn: []
}

resource tweetNewDeprecationWorkflowNameResource 'Microsoft.Logic/workflows@2019-05-01' = {
  name: tweetNewDeprecationWorkflowName
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
        When_a_new_notice_was_published: {
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'servicebus\'][\'connectionId\']'
              }
            }
            method: 'get'
            path: '/@{encodeURIComponent(encodeURIComponent(\'new-deprecation-notices\'))}/subscriptions/@{encodeURIComponent(\'new-notice-tweet\')}/messages/head/peek'
            queries: {
              sessionId: 'None'
              subscriptionType: 'Main'
            }
          }
          recurrence: {
            frequency: 'Minute'
            interval: 15
          }
        }
      }
      actions: {
        Complete_the_new_notice_to_tweet: {
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'servicebus\'][\'connectionId\']'
              }
            }
            method: 'delete'
            path: '/@{encodeURIComponent(encodeURIComponent(\'new-deprecation-notices\'))}/subscriptions/@{encodeURIComponent(\'new-notice-tweet\')}/messages/complete'
            queries: {
              lockToken: '@triggerBody()?[\'LockToken\']'
              sessionId: ''
              subscriptionType: 'Main'
            }
          }
          runAfter: {
            Post_a_tweet: [
              'Succeeded'
            ]
          }
        }
        Parse_JSON: {
          runAfter: {
          }
          type: 'ParseJson'
          inputs: {
            content: '@{decodeBase64(triggerBody()?[\'ContentData\'])}'
            schema: {
              DeprecationInfo: {
                AdditionalInformation: 'https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3'
                Contact: {
                  Type: 3
                }
                DueOn: '2025-03-31T00:00:00+00:00'
                Impact: {
                  Area: 'Feature'
                  Cloud: 'Public'
                  Description: 'Service will no longer be available.'
                  Services: [
                    'CognitiveServices'
                  ]
                  Type: 'MigrationRequired'
                }
                Notice: {
                  AdditionalInfo: null
                  Links: [
                    'https://azure.microsoft.com/en-gb/updates/version-2-of-translator-is-retiring-on-24-may-2021/'
                    'https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3'
                  ]
                  OfficialReport: '> In May 2018, we announced the general availability of version 3 of Translator and will retire version 2 of Translator on 24 May 2021.\r\n> \r\n> Key benefits of version 3 of Translator include: \r\n> - More functionalities including bilingual dictionary, transliterate and translate to multiple target languages in a single request.\r\n> - Provides a  [layered security model](https://docs.microsoft.com/en-us/azure/cognitive-services/Welcome#securing-resources) as part of Azure Cognitive Services.\r\n> - Customized translations through [Custom Translator](https://portal.customtranslator.azure.ai/).'
                }
                RequiredAction: {
                  AdditionalInfo: null
                  OfficialReport: '> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.\r\n> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal.'
                }
                Title: 'Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.'
              }
              PublishedNotice: {
                ApiInfo: {
                  Id: 704370266
                  Url: 'https://api.github.com/repos/azure-deprecation-automation/sandbox/issues/73'
                }
                ClosedAt: null
                CreatedAt: '2020-09-18T13:16:36+00:00'
                DashboardInfo: {
                  Id: 73
                  Url: 'https://github.com/azure-deprecation-automation/sandbox/issues/73'
                }
                Labels: [
                  'area:certification'
                  'impact:migration-required'
                  'services:cognitive-services'
                  'verified'
                ]
                Title: 'Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.'
                UpdatedAt: '2020-09-18T13:16:37+00:00'
              }
            }
          }
        }
        Post_a_tweet: {
          runAfter: {
            Parse_JSON: [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'twitter\'][\'connectionId\']'
              }
            }
            method: 'post'
            path: '/posttweet'
            queries: {
              tweetText: '📢 @{body(\'Parse_JSON\')[\'DeprecationInfo\']?[\'Title\']}\n\n@{body(\'Parse_JSON\')[\'PublishedNotice\']?[\'DashboardInfo\']?[\'Url\']}\n\n#azure #deprecation'
            }
          }
        }
      }
      outputs: {
      }
    }
    parameters: {
      '$connections': {
        value: {
          servicebus: {
            connectionId: serviceBusConnection.id
            connectionName: serviceBusConnectionName
            id: serviceBusConnectionId
          }
          twitter: {
            connectionId: twitterConnection.id
            connectionName: twitterConnectionName
            id: twitterConnectionId
          }
        }
      }
    }
  }
}

resource cosmosDbConnection 'Microsoft.Web/connections@2016-06-01' = {
  name: cosmosDbConnectionName
  location: defaultLocation
  properties: {
    displayName: cosmosDbConnectionName
    customParameterValues: {
    }
    api: {
      id: cosmosDbConnectionId
    }
    parameterValues: {
      databaseAccount: cosmosDbAccountName
      accessKey: listKeys(resourceId('Microsoft.DocumentDB/databaseAccounts', cosmosDbAccountName), '2015-04-08').primaryMasterKey
    }
  }
  dependsOn: []
}
