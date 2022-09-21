param applicationInsightsName string
param cosmosDbAccountName string
param cosmosDbCollectionName string
param cosmosDbDatabaseName string
param eventGridTopicName string
param logAnalyticsName string
param serviceBusNamespaceName string
param storageAccountName string

param defaultLocation string = resourceGroup().location
resource storageAccountNameResource 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: defaultLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'Storage'
  properties: {
  }
}

resource eventGridTopicNameResource 'Microsoft.EventGrid/topics@2021-06-01-preview' = {
  name: eventGridTopicName
  location: defaultLocation
  sku: {
    name: 'Basic'
  }
  kind: 'Azure'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    inputSchema: 'CloudEventSchemaV1_0'
    publicNetworkAccess: 'Enabled'
  }
}

resource logAnalyticsNameResource 'microsoft.operationalinsights/workspaces@2021-06-01' = {
  name: logAnalyticsName
  location: defaultLocation
  properties: {
    sku: {
      name: 'pergb2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: -1
    }
  }
}

resource cosmosDbAccountNameResource 'Microsoft.DocumentDB/databaseAccounts@2021-06-15' = {
  name: cosmosDbAccountName
  location: defaultLocation
  tags: {
  }
  kind: 'GlobalDocumentDB'
  identity: {
    type: 'None'
  }
  properties: {
    databaseAccountOfferType: 'Standard'
    publicNetworkAccess: 'Enabled'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations: [
      {
        locationName: 'West Europe'
        failoverPriority: 0
        isZoneRedundant: true
      }
    ]
    cors: []
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    ipRules: []
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
      }
    }
    networkAclBypassResourceIds: []
  }
}

resource cosmosDbAccountName_cosmosDbDatabaseName 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-06-15' = {
  parent: cosmosDbAccountNameResource
  name: cosmosDbDatabaseName
  properties: {
    resource: {
      id: cosmosDbDatabaseName
    }
  }
}

resource applicationInsightsNameResource 'microsoft.insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: defaultLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: 30
    WorkspaceResourceId: logAnalyticsNameResource.id
    IngestionMode: 'LogAnalytics'
  }
}

resource serviceBusNamespaceNameResource 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: serviceBusNamespaceName
  location: defaultLocation
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
  }
}

resource serviceBusNamespaceName_new_azure_deprecation 'Microsoft.ServiceBus/namespaces/queues@2021-06-01-preview' = {
  parent: serviceBusNamespaceNameResource
  name: 'new-azure-deprecation'
  properties: {
    maxDeliveryCount: 5
  }
}

resource serviceBusNamespaceName_new_deprecation_notices 'Microsoft.ServiceBus/namespaces/topics@2021-06-01-preview' = {
  parent: serviceBusNamespaceNameResource
  name: 'new-deprecation-notices'
  properties: {
  }
}

resource cosmosDbAccountName_cosmosDbDatabaseName_cosmosDbCollectionName 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-06-15' = {
  parent: cosmosDbAccountName_cosmosDbDatabaseName
  name: cosmosDbCollectionName
  properties: {
    resource: {
      id: 'deprecations'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

resource serviceBusNamespaceName_new_deprecation_notices_archive 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices
  name: 'archive'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 3
  }
}

resource serviceBusNamespaceName_new_deprecation_notices_archive_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices_archive
  name: 'unfiltered'
  location: defaultLocation
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

resource serviceBusNamespaceName_new_deprecation_notices_event_grid_notifications 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices
  name: 'event-grid-notifications'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 3
  }
}

resource serviceBusNamespaceName_new_deprecation_notices_event_grid_notifications_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices_event_grid_notifications
  name: 'unfiltered'
  location: defaultLocation
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

resource serviceBusNamespaceName_new_deprecation_notices_new_notice_tweet 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices
  name: 'new-notice-tweet'
  properties: {
    maxDeliveryCount: 3
  }
}

resource serviceBusNamespaceName_new_deprecation_notices_new_notice_tweet_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices_new_notice_tweet
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

resource serviceBusNamespaceName_new_deprecation_notices_persist_new_notice 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices
  name: 'persist-new-notice'
  properties: {
    maxDeliveryCount: 3
  }
}

resource serviceBusNamespaceName_new_deprecation_notices_persist_new_notice_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: serviceBusNamespaceName_new_deprecation_notices_persist_new_notice
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
