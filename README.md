# transaction-query-aggregator

## Description 

This API retrieves transactions from Azure File Shares. 

## Configuration

These are the following configuration items that can be set. Setting these items can be done through environment variables.

- "TransactionStoreConnectionStringCsv" - This required is a comma separated list of connection strings to azure file shares.
- "ShareName" - This required setting is the name of the share on each share to look in for transactions.

## Search algorithm 

### Azure folder structure

The folder structure used is designed to make it easier to query by date. Below is an example of a transaction in the Azure file store.

"2020/1/1/0/dd115591-075c-4c0d-94b0-6bf74cef6bbe"

This example is formed from the date "2020-01-01 00:00:00.000" for a file with the ID "dd115591-075c-4c0d-94b0-6bf74cef6bbe".

The format of this path is "[YYYY]/[MM]/[DD]/[HH]/[FileId]"

In each of these folders there are two files:

- metadata.json - A JSON object containing an array of event data objects. Each event data object is a dictionary of properties.
- report.xml - The analysis report generated by the Glasswall d-First Engine

### POST api/v1/transactions

This endpoint searches for metadata.json files that match the criteria specified by the client.

In parallel the API will search through multiple azure file shares using the [Azure.File.Storage.Files](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.storage.file?view=azure-dotnet-legacy) nuget package. Connection to each share is specified by the "TransactionStoreConnectionStringCsv" and "ShareName" environment settings. 

Asynchronously the API will recurse through the folder structure, download and deserialise the metadata.json files. If any part of the path does not fall into the date range the client is filtering on, it will not be downloaded. For example, before entering a folder for year 2020, it will check this year falls within the search range. Once it reaches the file ID folders, it will append the metadata.json file name to the path for download.

The optional filters require the file to be downloaded and its data checked before deciding to discard a transaction. (Such as file type and risk)

If file IDs are specified as a filter, the search will stop once it finds them all.

The following filters on transactions can be applied

- TimestampRangeStart: Required
- TimestampRangeEnd: Required
- FileTypes: Optional
- Risks: Optional
- PolicyIds: Optional
- FileIds: Optional

Sample Response:

```
{
    "count": 1,
    "files": [
        {
            "timestamp": "2020-10-01T00:06:01+00:00",
            "fileId": "103611f3-df3e-4745-8358-730b312c61d6",
            "detectionFileType": 23,
            "risk": 4,
            "activePolicyId": "65cc7c2b-cbe1-4442-87c4-fb075f833929",
            "directory": "2020/10/1/1/103611f3-df3e-4745-8358-730b312c61d6"
        }
    ]
}
```

### GET api/v1/transactions?filePath={filePath}

This endpoint gets the detail of a single transaction. The POST endpoint returns the directory for each file found, for use with this endpoint as parameter "filePath". With this path, it will pull down the report.xml and return that as a JSON object.

## Useful links

See [Static data generation tool](https://github.com/filetrust/transaction-query-aggregator-static-data) for a way to generate data to test this API with.

See the [Swagger](https://filetrust.github.io/transaction-query-aggregator/#/) page for usage.

# Local Setup

## Pre-requisites

### API

- Docker desktop with kubernetes
- aspnet core 3.1 or later
- Visual studio 

### Swagger

- vscode
- node/npm

# How to

## Debug the API locally

- Open TransactionQueryAggregator.sln
- Ensure docker-compose is the startup project
- Start docker compose
- API is reachable at http://localhost:32769 and https://localhost:32768

## Run Swagger Page locally

- Ensure API is running if swagger is to target your local API
- Open vscode
- Open a terminal in gh-page
- run `npm i` to install packages
- run `npm start` to launch the swagger page
- This should launch a browser at "http://localhost:3000/transaction-query-aggregator"
