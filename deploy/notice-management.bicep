param applicationInsightsName string
param cosmosDbConnectionName string
param serviceBusConnectionName string
param cosmosDbAccountName string
param functionAppName string
param functionPlanName string
param githubOwner string
param githubRepoName string

@secure()
param githubPersonalAccessToken string
param serviceBusNamespaceName string
param storageAccountName string
param persistDeprecationWorkflowName string

param defaultLocation string = resourceGroup().location
var cosmosDbConnectionId = subscriptionResourceId('Microsoft.Web/locations/managedApis', defaultLocation, 'documentdb')
var serviceBusConnectionId = subscriptionResourceId('Microsoft.Web/locations/managedApis', defaultLocation, 'servicebus')

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

resource functionAppNameResource 'Microsoft.Web/sites@2021-01-15' = {
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
          name: 'githubOwner'
          value: githubOwner
        }
        {
          name: 'GitHub_RepoName'
          value: githubRepoName
        }
        {
          name: 'githubPersonalAccessToken'
          value: githubPersonalAccessToken
        }
        {
          name: 'ServiceBus_ConnectionString'
          value: listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', serviceBusNamespaceName, 'RootManageSharedAccessKey'), '2017-04-01').primaryConnectionString
        }
      ]
    }
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource deprecationLifecycleUpdatesQueue 'Microsoft.ServiceBus/namespaces/queues@2021-06-01-preview' = {
  parent: serviceBusNamespace
  name: 'deprecation-lifecycle-updates'
  properties: {
    maxDeliveryCount: 5
  }
}

resource deprecationNoticeLifecycleTopic 'Microsoft.ServiceBus/namespaces/topics@2021-06-01-preview' = {
  parent: serviceBusNamespace
  name: 'deprecation-notice-lifecycle-changes'
  properties: {
  }
}

resource emitEventGridNotificationTopicSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: deprecationNoticeLifecycleTopic
  name: 'event-grid-notifications'
  properties: {
    maxDeliveryCount: 3
  }
}

resource emitEventGridNotificationTopicSubscriptionFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: emitEventGridNotificationTopicSubscription
  name: 'unfiltered'
  properties: {
    action: {
    }
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: '1=1'
      compatibilityLevel: 20
    }
  }
}

resource tweetDeprecationExtendedTopicSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: deprecationNoticeLifecycleTopic
  name: 'tweet-deprecation-extended'
  properties: {
    maxDeliveryCount: 3
  }
}

resource tweetDeprecationExtendedTopicSubscriptionFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: tweetDeprecationExtendedTopicSubscription
  name: 'unfiltered'
  properties: {
    action: {
    }
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: 'MessageType=\'AzureDeprecationWasExtendedV1\''
      compatibilityLevel: 20
    }
  }
}

resource tweetDeprecationClosureTopicSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: deprecationNoticeLifecycleTopic
  name: 'tweet-deprecation-closure'
  properties: {
    maxDeliveryCount: 3
  }
}

resource tweetDeprecationClosureTopicSubscriptionFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: tweetDeprecationClosureTopicSubscription
  name: 'unfiltered'
  properties: {
    action: {
    }
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: 'MessageType=\'AzureDeprecationWasClosedV1\''
      compatibilityLevel: 20
    }
  }
}

resource persistNoticeChangedTopicSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: deprecationNoticeLifecycleTopic
  name: 'persist-notice-update'
  properties: {
    maxDeliveryCount: 3
  }
}

resource persistNoticeChangeTopicSubscriptionFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: persistNoticeChangedTopicSubscription
  name: 'unfiltered'
  properties: {
    action: {
    }
    filterType: 'SqlFilter'
    sqlFilter: {
      sqlExpression: '1=1'
      compatibilityLevel: 20
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

resource persistDeprecationWorkflowNameResource 'Microsoft.Logic/workflows@2019-05-01' = {
  name: persistDeprecationWorkflowName
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
            connectionId: cosmosDbConnection.id
            connectionName: cosmosDbConnectionName
            id: cosmosDbConnectionId
          }
          servicebus: {
            connectionId: serviceBusConnection.id
            connectionName: serviceBusConnectionName
            id: serviceBusConnectionId
          }
        }
      }
    }
  }
}
