param ApplicationInsights_Name string
param CosmosDb_Account_Name string
param CosmosDb_Collection_Name string
param CosmosDb_Database_Name string
param EventGrid_Topic_Name string
param LogAnalytics_Name string
param ServiceBus_Namespace_Name string
param StorageAccount_Name string

param defaultLocation string = resourceGroup().location
resource StorageAccount_Name_resource 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: StorageAccount_Name
  location: defaultLocation
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  kind: 'Storage'
  properties: {
  }
}

resource EventGrid_Topic_Name_resource 'Microsoft.EventGrid/topics@2021-06-01-preview' = {
  name: EventGrid_Topic_Name
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

resource LogAnalytics_Name_resource 'microsoft.operationalinsights/workspaces@2021-06-01' = {
  name: LogAnalytics_Name
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

resource CosmosDb_Account_Name_resource 'Microsoft.DocumentDB/databaseAccounts@2021-06-15' = {
  name: CosmosDb_Account_Name
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

resource CosmosDb_Account_Name_CosmosDb_Database_Name 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-06-15' = {
  parent: CosmosDb_Account_Name_resource
  name: '${CosmosDb_Database_Name}'
  properties: {
    resource: {
      id: CosmosDb_Database_Name
    }
  }
}

resource ApplicationInsights_Name_resource 'microsoft.insights/components@2020-02-02' = {
  name: ApplicationInsights_Name
  location: defaultLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: 30
    WorkspaceResourceId: LogAnalytics_Name_resource.id
    IngestionMode: 'LogAnalytics'
  }
}

resource ServiceBus_Namespace_Name_resource 'Microsoft.ServiceBus/namespaces@2021-06-01-preview' = {
  name: ServiceBus_Namespace_Name
  location: defaultLocation
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
  }
}

resource ServiceBus_Namespace_Name_new_azure_deprecation 'Microsoft.ServiceBus/namespaces/queues@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_resource
  name: 'new-azure-deprecation'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 5
  }
}

resource ServiceBus_Namespace_Name_new_deprecation_notices 'Microsoft.ServiceBus/namespaces/topics@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_resource
  name: 'new-deprecation-notices'
  location: defaultLocation
  properties: {
  }
}

resource CosmosDb_Account_Name_CosmosDb_Database_Name_CosmosDb_Collection_Name 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-06-15' = {
  parent: CosmosDb_Account_Name_CosmosDb_Database_Name
  name: CosmosDb_Collection_Name
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

resource ServiceBus_Namespace_Name_new_deprecation_notices_archive 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices
  name: 'archive'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 3
  }
}

resource ServiceBus_Namespace_Name_new_deprecation_notices_archive_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices_archive
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

resource ServiceBus_Namespace_Name_new_deprecation_notices_event_grid_notifications 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices
  name: 'event-grid-notifications'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 3
  }
}

resource ServiceBus_Namespace_Name_new_deprecation_notices_event_grid_notifications_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices_event_grid_notifications
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

resource ServiceBus_Namespace_Name_new_deprecation_notices_new_notice_tweet 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices
  name: 'new-notice-tweet'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 3
  }
}

resource ServiceBus_Namespace_Name_new_deprecation_notices_new_notice_tweet_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices_new_notice_tweet
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

resource ServiceBus_Namespace_Name_new_deprecation_notices_persist_new_notice 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices
  name: 'persist-new-notice'
  location: defaultLocation
  properties: {
    maxDeliveryCount: 3
  }
}

resource ServiceBus_Namespace_Name_new_deprecation_notices_persist_new_notice_unfiltered 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: ServiceBus_Namespace_Name_new_deprecation_notices_persist_new_notice
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
