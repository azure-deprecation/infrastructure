param applicationInsightsName string
param cosmosDbAccountName string
param cosmosDbDatabaseName string
param cosmosDbCollectionName string
param eventGridTopicName string
param logAnalyticsName string
param serviceBusNamespaceName string
param storageAccountName string
param apiManagementInstanceName string
param apiManagementSku string
param apiManagementAdminEmail string
param apiManagementAdminOrganization string

param defaultLocation string = resourceGroup().location
resource apiManagementInstance 'Microsoft.ApiManagement/service@2021-12-01-preview' = {
  name: apiManagementInstanceName
  location: defaultLocation
  sku: {
    name: apiManagementSku
    capacity: 1
  }
  properties: {
    publisherEmail: apiManagementAdminEmail
    publisherName: apiManagementAdminOrganization
    disableGateway: false
    publicNetworkAccess: 'Enabled'
  }
}
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: defaultLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'Storage'
  properties: {
  }
}

resource eventGridTopic 'Microsoft.EventGrid/topics@2021-06-01-preview' = {
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

resource logAnalyticsWorkspace 'microsoft.operationalinsights/workspaces@2021-06-01' = {
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

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-06-15' = {
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

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-06-15' = {
  parent: cosmosDbAccount
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
    WorkspaceResourceId: logAnalyticsWorkspace.id
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

resource newDeprecationQueue 'Microsoft.ServiceBus/namespaces/queues@2021-06-01-preview' = {
  parent: serviceBusNamespaceNameResource
  name: 'new-azure-deprecation'
  properties: {
    maxDeliveryCount: 5
  }
}

resource newDeprecationNoticesTopic 'Microsoft.ServiceBus/namespaces/topics@2021-06-01-preview' = {
  parent: serviceBusNamespaceNameResource
  name: 'new-deprecation-notices'
  properties: {
  }
}

resource cosmosDbCollection 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-06-15' = {
  parent: cosmosDbDatabase
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

resource archiveNewDeprecationsTopicSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2021-06-01-preview' = {
  parent: newDeprecationNoticesTopic
  name: 'archive'
  properties: {
    maxDeliveryCount: 3
  }
}

resource archiveNewDeprecationsTopicSubscriptionFilter 'Microsoft.ServiceBus/namespaces/topics/subscriptions/rules@2021-06-01-preview' = {
  parent: archiveNewDeprecationsTopicSubscription
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
