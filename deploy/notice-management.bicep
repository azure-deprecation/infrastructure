param applicationInsightsName string
param cosmosDbAccountName string
param cosmosDbDatabaseName string
param cosmosDbCollectionName string
param functionAppName string
param functionPlanName string
param githubOwner string
param githubRepoName string

@secure()
param githubPersonalAccessToken string
param serviceBusNamespaceName string
param storageAccountName string

param defaultLocation string = resourceGroup().location

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
          name: 'GitHub_Owner'
          value: githubOwner
        }
        {
          name: 'GitHub_RepoName'
          value: githubRepoName
        }
        {
          name: 'GitHub_Token'
          value: githubPersonalAccessToken
        }
        {
          name: 'ServiceBus_ConnectionString'
          value: listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', serviceBusNamespaceName, 'RootManageSharedAccessKey'), '2017-04-01').primaryConnectionString
        }
        {
          name: 'CosmosDb_DatabaseName'
          value: cosmosDbDatabaseName
        }
        {
          name: 'CosmosDb_ContainerName'
          value: cosmosDbCollectionName
        }
        {
          name: 'CosmosDb_ConnectionString'
          value: first(listConnectionStrings(resourceId('Microsoft.DocumentDB/databaseAccounts', cosmosDbAccountName), '2019-12-12').connectionStrings).connectionString
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
