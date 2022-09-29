param applicationInsightsName string
param functionAppName string
param functionPlanName string
param storageAccountName string
param cosmosDbAccountName string
param cosmosDbDatabaseName string
param cosmosDbCollectionName string
param apiManagementInstanceName string = 'azure-deprecation-notices-staging'

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

// API Gateway
resource apiManagementInstance 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: apiManagementInstanceName
}

resource restApi 'Microsoft.ApiManagement/service/apis@2021-12-01-preview' = {
  parent: apiManagementInstance
  name: 'azure-deprecation-api-rest'
  properties: {
    displayName: 'Azure Deprecation API (REST)'
    apiRevision: '1'
    description: 'APIs to explore the deprecations in Microsoft Azure.'
    subscriptionRequired: true
    serviceUrl: 'https://azure-deprecation-notices-rest-api-staging.azurewebsites.net/api/v1/deprecations'
    path: 'rest/deprecations'
    protocols: [
      'http'
      'https'
    ]
    isCurrent: true
  }
}

resource graphQlApi 'Microsoft.ApiManagement/service/apis@2021-12-01-preview' = {
  parent: apiManagementInstance
  name: 'deprecation-notices-api-graphql'
  properties: {
    displayName: 'Deprecation Notices API (GraphQL)'
    apiRevision: '1'
    subscriptionRequired: true
    path: 'graphql/deprecations'
    protocols: [
      'http'
      'https'
    ]
    type: 'graphql'
    isCurrent: true
  }
}

resource deprecationNoticeProduct 'Microsoft.ApiManagement/service/products@2021-12-01-preview' = {
  parent: apiManagementInstance
  name: 'deprecation-notices'
  properties: {
    displayName: 'Deprecation Notices'
    description: 'All APIs around deprecation notices.'
    subscriptionRequired: true
    approvalRequired: true
    state: 'published'
  }
}

resource restApiKeyNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-12-01-preview' = {
  parent: apiManagementInstance
  name: 'rest-api-function-api-key'
  properties: {
    displayName: 'rest-api-function-api-key'
    secret: true
    value: listkeys('${functionAppResource.id}/host/default', '2016-08-01').masterKey
  }
}

resource restApiOnFunctionsBackend 'Microsoft.ApiManagement/service/backends@2021-12-01-preview' = {
  parent: apiManagementInstance
  name: 'rest-api-on-azure-functions'
  properties: {
    url: 'https://${functionAppName}.azurewebsites.net'
    protocol: 'http'
    resourceId: 'https://management.azure.com${functionAppResource.id}'
    credentials: {
      query: {
      }
      header: {
        'x-functions-key': [
          '{{${restApiKeyNamedValue.name}}}'
        ]
      }
    }
    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource deprecationsTag 'Microsoft.ApiManagement/service/tags@2021-12-01-preview' = {
  parent: apiManagementInstance
  name: 'deprecations'
  properties: {
    displayName: 'deprecations'
  }
}

resource getDeprecationRestOperation 'Microsoft.ApiManagement/service/apis/operations@2021-12-01-preview' = {
  parent: restApi
  name: 'GetDeprecation'
  properties: {
    displayName: 'Get deprecation details'
    method: 'GET'
    urlTemplate: '/api/v1/deprecations/{id}'
    templateParameters: [
      {
        name: 'id'
        description: 'The unique ID of the deprecation.'
        type: 'string'
        required: true
        values: []
      }
    ]
    description: 'Provides capability to get detailed about a specific deprecation'
    responses: [
      {
        statusCode: 200
        description: 'Payload of DeprecationInfo'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {
                }
              }
            }
            typeName: 'deprecationInfo'
          }
        ]
        headers: []
      }
    ]
  }
}

resource getDeprecationsRestOperation 'Microsoft.ApiManagement/service/apis/operations@2021-12-01-preview' = {
  parent: restApi
  name: 'GetDeprecations'
  properties: {
    displayName: 'Get all deprecations'
    method: 'GET'
    urlTemplate: '/api/v1/deprecations'
    templateParameters: []
    description: 'Provides capability to browse all deprecations'
    request: {
      queryParameters: [
        {
          name: 'filters.status'
          description: 'Filter to reduce deprecation notices by a given status.'
          type: 'string'
          values: []
        }
        {
          name: 'filters.year'
          description: 'Filter to reduce deprecation notices by the year of the deprecation.'
          type: 'string'
          values: []
        }
        {
          name: 'filters.service'
          description: 'Filter to reduce deprecation notices for a given Azure service.'
          type: 'string'
          values: []
        }
        {
          name: 'filters.impactType'
          description: 'Filter to reduce deprecation notices by a given impact type.'
          type: 'string'
          values: []
        }
        {
          name: 'filters.cloud'
          description: 'Filter to reduce deprecation notices for a given cloud.'
          type: 'string'
          values: []
        }
        {
          name: 'pagination.offset'
          description: 'Specifies the amount of pages to skip.'
          type: 'string'
          values: []
        }
        {
          name: 'pagination.limit'
          description: 'Specifies the amount of entries in the page.'
          type: 'string'
          values: []
        }
      ]
      headers: []
      representations: []
    }
    responses: [
      {
        statusCode: 200
        description: 'Payload of DeprecationNoticesResponse'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {
                }
              }
            }
            typeName: 'deprecationNoticesResponse'
          }
        ]
        headers: []
      }
    ]
  }
}

resource restApiAllPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-12-01-preview' = {
  parent: restApi
  name: 'policy'
  properties: {
    value: '<policies>\r\n  <inbound>\r\n    <base />\r\n    <rate-limit calls="60" renewal-period="60" retry-after-header-name="x-ratelimit-reset" remaining-calls-header-name="x-ratelimit-remaining" total-calls-header-name="x-ratelimit-limit" />\r\n  </inbound>\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
}

resource graphQlApiAllPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-12-01-preview' = {
  parent: graphQlApi
  name: 'policy'
  properties: {
    value: '<!--\r\n    IMPORTANT:\r\n    - Policy elements can appear only within the <inbound>, <outbound>, <backend> section elements.\r\n    - To apply a policy to the incoming request (before it is forwarded to the backend service), place a corresponding policy element within the <inbound> section element.\r\n    - To apply a policy to the outgoing response (before it is sent back to the caller), place a corresponding policy element within the <outbound> section element.\r\n    - To add a policy, place the cursor at the desired insertion point and select a policy from the sidebar.\r\n    - To remove a policy, delete the corresponding policy statement from the policy document.\r\n    - Position the <base> element within a section element to inherit all policies from the corresponding section element in the enclosing scope.\r\n    - Remove the <base> element to prevent inheriting policies from the corresponding section element in the enclosing scope.\r\n    - Policies are applied in the order of their appearance, from the top down.\r\n    - Comments within policy elements are not supported and may disappear. Place your comments between policy elements or at a higher level scope.\r\n-->\r\n<policies>\r\n  <inbound>\r\n    <base />\r\n  </inbound>\r\n  <backend>\r\n    <set-graphql-resolver parent-type="Query" field="getDeprecations">\r\n      <http-data-source>\r\n        <http-request>\r\n          <set-method>GET</set-method>\r\n          <set-url>@{\r\n                    \r\n                        var body = context.Request.Body.As&lt;JObject&gt;(true);\r\n                        var sb = new StringBuilder();\r\n\r\n                        sb.Append("?code=WHrQ94DgDEwtdSTDr-wIBJ-7y7DHyP6NpuhZ0xk2wFpdAzFuHEB-oQ==");\r\n\r\n                        const string filterPrefix = "filters."; \r\n                        var needPrefix = new HashSet&lt;string&gt;() {\r\n                             "status", \r\n                            //"service",\r\n                            // "year", \r\n                            // "impactType", \r\n                            // "area", \r\n                            // "cloud"\r\n                        };\r\n\r\n                        if (body.TryGetValue("variables", out var vars)) {\r\n                            var jVars = vars as JObject;\r\n                            if (jVars != null) {\r\n                                foreach (var prop in jVars.Properties())\r\n                                {                                    \r\n                                    var prefix = needPrefix.Contains(prop.Name) ? filterPrefix : string.Empty;\r\n                                    //var prefix = "filters.";\r\n                                    sb.Append("&amp;" + prefix + prop.Name + "=" + prop.Value.ToString());\r\n                                }  \r\n                            }\r\n                        }\r\n\r\n                        return "https://azure-deprecation-notices-rest-api-staging.azurewebsites.net/api/v1/deprecations" + sb.ToString();\r\n                    }</set-url>\r\n        </http-request>\r\n      </http-data-source>\r\n    </set-graphql-resolver>\r\n    <set-graphql-resolver parent-type="Query" field="getDeprecation">\r\n      <http-data-source>\r\n        <http-request>\r\n          <set-method>GET</set-method>\r\n          <set-url>@{\r\n\t\t                var body = context.Request.Body.As&lt;JObject&gt;(true);\r\n\t\t                return "https://azure-deprecation-notices-rest-api-staging.azurewebsites.net/api/v1/deprecations/" +      \r\n                            body["variables"]["id"].ToString() + "?code=WHrQ94DgDEwtdSTDr-wIBJ-7y7DHyP6NpuhZ0xk2wFpdAzFuHEB-oQ==";\r\n                    }</set-url>\r\n        </http-request>\r\n      </http-data-source>\r\n    </set-graphql-resolver>\r\n    <base />\r\n  </backend>\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
}

resource deprecationNoticeProductWithRestApi 'Microsoft.ApiManagement/service/products/apis@2021-12-01-preview' = {
  parent: deprecationNoticeProduct
  name: 'azure-deprecation-api-rest'
}

resource deprecationNoticeProductWithGraphQlApi 'Microsoft.ApiManagement/service/products/apis@2021-12-01-preview' = {
  parent: deprecationNoticeProduct
  name: 'deprecation-notices-api-graphql'
}

resource getDeprecationRestOperationPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-12-01-preview' = {
  parent: getDeprecationRestOperation
  name: 'policy'
  properties: {
    value: '<policies>\r\n  <inbound>\r\n    <base />\r\n    <set-backend-service backend-id="${restApiOnFunctionsBackend.name}" />\r\n    <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />\r\n  </inbound>\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <outbound>\r\n    <base />\r\n    <cache-store duration="3600" />\r\n  </outbound>\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
}

resource getDeprecationsRestOperationPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-12-01-preview' = {
  parent: getDeprecationsRestOperation
  name: 'policy'
  properties: {
    value: '<policies>\r\n  <inbound>\r\n    <base />\r\n  <set-backend-service backend-id="${restApiOnFunctionsBackend.name}" />\r\n    <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />\r\n  </inbound>\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <outbound>\r\n    <base />\r\n    <cache-store duration="3600" />\r\n  </outbound>\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
}

resource getDeprecationRestOperationDeprecationsTag 'Microsoft.ApiManagement/service/apis/operations/tags@2021-12-01-preview' = {
  parent: getDeprecationRestOperation
  name: 'deprecations'
}

resource getDeprecationsRestOperationDeprecationsTag 'Microsoft.ApiManagement/service/apis/operations/tags@2021-12-01-preview' = {
  parent: getDeprecationsRestOperation
  name: 'deprecations'
}

