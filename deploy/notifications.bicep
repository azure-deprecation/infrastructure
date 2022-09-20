param ApplicationInsights_Name string
param Connections_CosmosDb_Name string
param Connections_Gmail_Name string
param Connections_MailChimp_Name string
param Connections_ServiceBus_Name string
param Connections_Twitter_Name string
param CosmosDb_Account_Name string
param Function_App_Name string
param Function_Plan_Name string
param EventGrid_Topic_Name string
param Gmail_Auth_ClientId string

@secure()
param Gmail_Auth_ClientSecret string

@secure()
param MailChimp_Auth_BearerToken string
param MailChimp_MailingList_Id string
param MailChimp_Template_Id int
param ServiceBus_Namespace_Name string
param StorageAccount_Name string
param Workflow_MonthlySummaryNewsletter_Name string
param Workflow_TweetNewDeprecation_Name string

param defaultLocation string = resourceGroup().location
var cosmosDbConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/documentdb'
var gmailConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/gmail'
var mailChimpConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/mailchimp'
var serviceBusConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/servicebus'
var twitterConnectionId = '${subscription().id}/providers/Microsoft.Web/locations/${defaultLocation}/managedApis/twitter'

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
          name: 'EVENTGRID_AUTH_KEY'
          value: listKeys(resourceId('Microsoft.EventGrid/topics', EventGrid_Topic_Name), '2020-06-01').key1
        }
        {
          name: 'EVENTGRID_ENDPOINT'
          value: 'https://${EventGrid_Topic_Name}.${defaultLocation}-1.eventgrid.azure.net/api/events'
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

resource Connections_Twitter_Name_resource 'Microsoft.Web/connections@2016-06-01' = {
  name: Connections_Twitter_Name
  location: defaultLocation
  properties: {
    displayName: Connections_Twitter_Name
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

resource Workflow_TweetNewDeprecation_Name_resource 'Microsoft.Logic/workflows@2017-07-01' = {
  name: Workflow_TweetNewDeprecation_Name
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
            connectionId: Connections_ServiceBus_Name_resource.id
            connectionName: Connections_ServiceBus_Name
            id: serviceBusConnectionId
          }
          twitter: {
            connectionId: Connections_Twitter_Name_resource.id
            connectionName: Connections_Twitter_Name
            id: twitterConnectionId
          }
        }
      }
    }
  }
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

resource Connections_MailChimp_Name_resource 'Microsoft.Web/connections@2016-06-01' = {
  name: Connections_MailChimp_Name
  location: defaultLocation
  properties: {
    displayName: Connections_MailChimp_Name
    customParameterValues: {
    }
    api: {
      id: mailChimpConnectionId
    }
    parameterValues: {
    }
  }
  dependsOn: []
}

resource Connections_Gmail_Name_resource 'Microsoft.Web/connections@2018-07-01-preview' = {
  name: Connections_Gmail_Name
  location: defaultLocation
  kind: 'V1'
  properties: {
    api: {
      id: gmailConnectionId
    }
    displayName: Connections_Gmail_Name
    parameterValueSet: {
      name: 'byoa'
      values: {
        'token-byoa:clientId': {
          value: Gmail_Auth_ClientId
        }
        'token-byoa:clientSecret': {
          value: Gmail_Auth_ClientSecret
        }
        'token-byoa': {
          value: 'https://global.consent.azure-apim.net/redirect'
        }
      }
    }
  }
}

resource Workflow_MonthlySummaryNewsletter_Name_resource 'Microsoft.Logic/workflows@2017-07-01' = {
  name: Workflow_MonthlySummaryNewsletter_Name
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
        'MailChimp.BearerToken': {
          defaultValue: MailChimp_Auth_BearerToken
          type: 'SecureString'
        }
        'MailChimp.MailingList.Id': {
          defaultValue: MailChimp_MailingList_Id
          type: 'String'
        }
        'MailChimp.Template.Id': {
          defaultValue: MailChimp_Template_Id
          type: 'Int'
        }
      }
      triggers: {
        Recurrence: {
          recurrence: {
            frequency: 'Month'
            interval: 1
            startTime: '2021-11-01T02:00:00Z'
            timeZone: 'Romance Standard Time'
          }
          evaluatedRecurrence: {
            frequency: 'Month'
            interval: 1
            startTime: '2021-11-01T02:00:00Z'
            timeZone: 'Romance Standard Time'
          }
          type: 'Recurrence'
        }
      }
      actions: {
        'Close_HTML_List_(New)': {
          runAfter: {
            For_Each_New_Deprecation: [
              'Succeeded'
            ]
          }
          type: 'AppendToStringVariable'
          inputs: {
            name: 'NewDeprecationsHtml'
            value: '</ul>'
          }
        }
        'Close_HTML_List_(Upcoming)': {
          runAfter: {
            For_Each_Upcoming_Deprecation: [
              'Succeeded'
            ]
          }
          type: 'AppendToStringVariable'
          inputs: {
            name: 'UpcomingDeprecationsHtml'
            value: '</ul>'
          }
        }
        Compose_Email_Campaign_Name: {
          runAfter: {
            'Close_HTML_List_(New)': [
              'Succeeded'
            ]
            'Close_HTML_List_(Upcoming)': [
              'Succeeded'
            ]
          }
          type: 'Compose'
          inputs: 'Azure Deprecation Notices - Monthly Summary (@{formatDateTime(utcNow(), \'MMMM yyyy\')})'
        }
        Create_New_Campaign: {
          runAfter: {
            Compose_Email_Campaign_Name: [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
          inputs: {
            body: {
              recipients: {
                list_id: '@parameters(\'MailChimp.MailingList.Id\')'
              }
              settings: {
                from_name: 'Azure Deprecation Notices'
                reply_to: 'no-reply@azure-deprecation-notices.cloud'
                subject_line: '@{outputs(\'Compose_Email_Campaign_Name\')}'
                title: '@{outputs(\'Compose_Email_Campaign_Name\')}'
              }
              type: 'regular'
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'mailchimp\'][\'connectionId\']'
              }
            }
            method: 'post'
            path: '/v2/campaigns'
          }
        }
        For_Each_New_Deprecation: {
          foreach: '@body(\'Get_New_Deprecations_From_Last_Month\')?[\'value\']'
          actions: {
            Append_New_Deprecation_To_List: {
              runAfter: {
              }
              type: 'AppendToStringVariable'
              inputs: {
                name: 'NewDeprecationsHtml'
                value: '<li>@{items(\'For_Each_New_Deprecation\')[\'Title\']} (<a href="@{items(\'For_Each_New_Deprecation\')[\'Url\']}">link</a>)</li>'
              }
            }
          }
          runAfter: {
            Get_New_Deprecations_From_Last_Month: [
              'Succeeded'
            ]
          }
          type: 'Foreach'
        }
        For_Each_Upcoming_Deprecation: {
          foreach: '@body(\'Get_First_10_Upcoming_Deprecations\')?[\'value\']'
          actions: {
            Append_Upcoming_Deprecation_To_List: {
              runAfter: {
              }
              type: 'AppendToStringVariable'
              inputs: {
                name: 'UpcomingDeprecationsHtml'
                value: '<li>@{items(\'For_Each_Upcoming_Deprecation\')[\'Title\']} (<a href="@{items(\'For_Each_Upcoming_Deprecation\')[\'Url\']}">link</a>)</li>'
              }
            }
          }
          runAfter: {
            Get_First_10_Upcoming_Deprecations: [
              'Succeeded'
            ]
          }
          type: 'Foreach'
        }
        Get_First_10_Upcoming_Deprecations: {
          runAfter: {
            Initialize_Upcoming_Deprecations_HTML: [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'documentdb\'][\'connectionId\']'
              }
            }
            method: 'get'
            path: '/v5/cosmosdb/@{encodeURIComponent(\'AccountNameFromSettings\')}/dbs/@{encodeURIComponent(\'deprecation-db\')}/colls/@{encodeURIComponent(\'deprecations\')}/query'
            queries: {
              queryText: 'SELECT c.DeprecationInfo.Title, c.PublishedNotice.DashboardInfo.Url FROM c ORDER BY c.DeprecationInfo.Timeline ASC'
            }
          }
        }
        Get_New_Deprecations_From_Last_Month: {
          runAfter: {
            Initialize_Upcoming_Deprecations_HTML: [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
          inputs: {
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'documentdb\'][\'connectionId\']'
              }
            }
            method: 'get'
            path: '/v5/cosmosdb/@{encodeURIComponent(\'AccountNameFromSettings\')}/dbs/@{encodeURIComponent(\'deprecation-db\')}/colls/@{encodeURIComponent(\'deprecations\')}/query'
            queries: {
              queryText: 'SELECT c.DeprecationInfo.Title, c.PublishedNotice.DashboardInfo.Url FROM c WHERE c.PublishedNotice.UpdatedAt >= \'@{formatDateTime(variables(\'PastPeriod\'), \'yyyy-MM\')}-01T00:00:01+00:00\' AND c.PublishedNotice.CreatedAt < \'@{formatDateTime(utcNow(), \'yyyy-MM\')}-01T00:00:01+00:00\' ORDER BY c.DeprecationInfo.Timeline ASC'
            }
          }
        }
        Initialize_New_Deprecations_HTML: {
          runAfter: {
            Initialize_Past_Period_Variable: [
              'Succeeded'
            ]
          }
          type: 'InitializeVariable'
          inputs: {
            variables: [
              {
                name: 'NewDeprecationsHtml'
                type: 'string'
                value: '<ul>'
              }
            ]
          }
        }
        Initialize_Past_Period_Variable: {
          runAfter: {
          }
          type: 'InitializeVariable'
          inputs: {
            variables: [
              {
                name: 'PastPeriod'
                type: 'string'
                value: '@{subtractFromTime(utcNow(), 1, \'Month\')}'
              }
            ]
          }
        }
        Initialize_Upcoming_Deprecations_HTML: {
          runAfter: {
            Initialize_New_Deprecations_HTML: [
              'Succeeded'
            ]
          }
          type: 'InitializeVariable'
          inputs: {
            variables: [
              {
                name: 'UpcomingDeprecationsHtml'
                type: 'string'
                value: '<ul>'
              }
            ]
          }
        }
        Show_New_Deprecations: {
          inputs: '@variables(\'NewDeprecationsHtml\')'
          runAfter: {
            Create_New_Campaign: [
              'Succeeded'
            ]
          }
          type: 'Compose'
        }
        Show_Upcoming_Deprecations: {
          inputs: '@variables(\'UpcomingDeprecationsHtml\')'
          runAfter: {
            Show_New_Deprecations: [
              'Succeeded'
            ]
          }
          type: 'Compose'
        }
        Send_Notification_Email: {
          inputs: {
            body: {
              Body: '<p>New deprecations:</p>\n@{variables(\'NewDeprecationsHtml\')}\n<p>Upcoming deprecations:</p>\n@{variables(\'UpcomingDeprecationsHtml\')}'
              Subject: '[Action Required] @{outputs(\'Compose_Email_Campaign_Name\')}'
              Cc: 'tomkerkhove.opensource@gmail.com'
              Importance: 'High'
              To: 'kerkhove.tom@gmail.com'
            }
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'gmail\'][\'connectionId\']'
              }
            }
            method: 'post'
            path: '/v2/Mail'
          }
          runAfter: {
            Show_Upcoming_Deprecations: [
              'Succeeded'
            ]
          }
          type: 'ApiConnection'
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
          mailchimp: {
            connectionId: Connections_MailChimp_Name_resource.id
            connectionName: Connections_MailChimp_Name
            id: mailChimpConnectionId
          }
          gmail: {
            connectionId: Connections_Gmail_Name_resource.id
            connectionName: Connections_Gmail_Name
            id: gmailConnectionId
          }
        }
      }
    }
  }
}
