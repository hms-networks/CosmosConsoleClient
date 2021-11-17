# Cosmos Console Client

## Description

A basic command line utility for interacting with an instance of Microsoft's
`Cosmos DB`.

> __NOTICE__: This utility is primarily focused on integration with other .NET
  applications. As such, there is minimal error handling provided by the utility
  meaning that exceptions that are thrown by the underlying Cosmos DB APIs will
  be thrown up to the command line.

## Setup

To utilize this application, the runtime (or SDK for compilation) of
`.NET v6.0` must be installed. The runtime installer is available here:
[.NET Installers](https://dotnet.microsoft.com/download/visual-studio-sdks)

To initialize and build the release, run the following:

```cmd
dotnet restore
dotnet build -c Release
```

### Cosmos DB Connection Settings

The Cosmos DB connection string and key are stored in the `App.config` file.
Upon building the project this file is copied and renamed to the binary name
with a `.config` extension (i.e. `CosmosConsoleClient.dll.config`). This file
is, by default, configured with the Cosmos DB Emulator default connection
settings, but can be modified as needed to connect to any Cosmos DB instance.

## Usage

To run the command and retrieve the top level `help` information, run:

```cmd
cd bin\Release\net6.0
dotnet .\CosmosConsoleClient.dll --help
```

Example item creation with initialization logic

```cmd
dotnet .\CosmosConsoleClient.dll item -i --db test --cid users -c "{'id':'1','name':'test','address':'here'}"
```

The above statement will initialize/create a `database` called "test" and a
`container` within the database called "users" using the default partition of
`/id` if either do not already exist. Then it will create a new entry in this
container consisting of the required "id" element for the partition and other
data.
