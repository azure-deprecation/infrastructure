# API Specs

This folder contains API specs for both REST (OpenAPI) and GraphQL schema.

## Generating GraphQL schema

To generate the latest GraphQL schema, use [`openapi-to-graphql-cli`](https://github.com/ibm/openapi-to-graphql) by running the following command:

```shell
$ openapi-to-graphql .\api-specs.json --save api-schema.sdl
{
  "warnings": [],
  "numOps": 2,
  "numOpsQuery": 2,
  "numOpsMutation": 0,
  "numOpsSubscription": 0,
  "numQueriesCreated": 2,
  "numMutationsCreated": 0,
  "numSubscriptionsCreated": 0
}
OpenAPI-to-GraphQL successfully saved your schema at api-schema.sdl
```
