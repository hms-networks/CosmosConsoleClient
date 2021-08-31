using System;
using System.Net;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosConsoleClient
{
    /// <summary>
    ///     Command for item management and interaction.
    /// </summary>
    public static class ItemCommand
    {
        /// <summary>
        ///     Specifies the command structure and handling.
        /// </summary>
        private static Command Command { get; } = new Command(
            name: ItemStrings.Command,
            description: Properties.Resources.CmdDescItem)
        {
            new Option(
                aliases: new string[] { ItemStrings.InitAll, ItemStrings.InitAllShort },
                description: Properties.Resources.ArgDescItemInitAll,
                getDefaultValue: () => false,
                arity: ArgumentArity.Zero)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ItemStrings.DatabaseId, ItemStrings.DatabaseIdShort, ItemStrings.DatabaseIdShortAlt },
                description: Properties.Resources.ArgDescItemDatabaseId,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = true
            },
            new Option(
                aliases: new string[] { ItemStrings.ContainerId, ItemStrings.ContainerIdShort },
                description: Properties.Resources.ArgDescItemContainerId,
                argumentType: typeof(string),
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = true
            },
            new Option(
                aliases: new string[] { ItemStrings.PartitionPath },
                description: Properties.Resources.ArgDescItemPartitionPath,
                argumentType: typeof(string),
                getDefaultValue: () => "/id",
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ItemStrings.PartitionKey, ItemStrings.PartitionKeyShort },
                description: Properties.Resources.ArgDescItemPartitionKey,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ItemStrings.CreateItem, ItemStrings.CreateItemShort },
                description: Properties.Resources.ArgDescItemCreateItem,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ItemStrings.DeleteId, ItemStrings.DeleteIdShort },
                description: Properties.Resources.ArgDescItemDeleteId,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ItemStrings.Throughput, ItemStrings.ThroughputShort },
                description: Properties.Resources.ArgDescItemThroughput,
                argumentType: typeof(int),
                getDefaultValue: () => 400,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
        };

        /// <summary>
        ///     Gets or sets a value indicating whether the command has been initialized
        ///     and assigned to a root command object.
        /// </summary>
        private static bool Initialized { get; set; } = false;

        /// <summary>
        ///     Performs initialization logic of the command and adds it to <paramref name="root"/>.
        /// </summary>
        /// <param name="root">
        ///     The root command to add this command to.
        /// </param>
        public static void Register(RootCommand root)
        {
            if (Initialized) { return; }

            Command.Handler = CommandHandler.Create<bool, string, string, string, string, string, string, int>(ExecuteCommandAsync);

            Command.AddValidator(cmd => {
                string result = null;
                bool createPresent = cmd.Children.Contains(ItemStrings.CreateItem) ||
                    cmd.Children.Contains(ItemStrings.CreateItemShort);
                bool deletePresent = cmd.Children.Contains(ItemStrings.DeleteId) ||
                    cmd.Children.Contains(ItemStrings.DeleteIdShort);

                if (!(createPresent ^ deletePresent))
                {
                    result = string.Format(
                        Properties.Resources.ValidateErrMsgCreateXorDelete,
                        ItemStrings.CreateItem,
                        ItemStrings.DeleteId);
                }

                return result;
            });

            root.AddCommand(Command);

            Initialized = true;
        }

        /// <summary>
        ///     The command execution logic.
        /// </summary>
        /// <param name="initAll">
        ///     When set <see langword="true"/>, the command will create the database and/or container if it does not exist.
        /// </param>
        /// <param name="databaseId">The ID of the database to interact with.</param>
        /// <param name="containerId">The ID of the container to interact with.</param>
        /// <param name="partitionPath">The partition path. Only used if a container is to be created.</param>
        /// <param name="partitionKey">The partition key value.</param>
        /// <param name="createItem">The JSON item to create/add to the container.</param>
        /// <param name="deleteId">The ID of the item to delete.</param>
        /// <param name="throughput">The throughput (RU/sec).</param>
        /// <returns>
        ///     <see langword="true"/> on command execution success;
        ///     otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <exception cref="NotInitializedException"/>
        /// <exception cref="NotImplementedException"/>
        private static async Task<bool> ExecuteCommandAsync(
            bool initAll,
            string databaseId,
            string containerId,
            string partitionPath,
            string partitionKey,
            string createItem,
            string deleteId,
            int throughput)
        {
            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            bool result;
            double requestCharge;
            HttpStatusCode statusCode;
            string endpointUri = ConfigurationManager.AppSettings[AppStrings.EndPointUri];
            string primaryKey = ConfigurationManager.AppSettings[AppStrings.PrimaryKey];

            using (var client = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = AppStrings.AppName }))
            {
                Container container;

                if (initAll)
                {
                    Database db = await client.CreateDatabaseIfNotExistsAsync(databaseId);
                    container = await db.CreateContainerIfNotExistsAsync(containerId, partitionPath, throughput);
                }
                else
                {
                    container = client.GetDatabase(databaseId).GetContainer(containerId);
                }

                if (!string.IsNullOrWhiteSpace(createItem))
                {
                    var data = JObject.Parse(createItem);
                    var response = await container.CreateItemAsync(data);
                    result = (response.StatusCode == HttpStatusCode.Created) || (response.StatusCode == HttpStatusCode.OK);
                    statusCode = response.StatusCode;
                    requestCharge = response.RequestCharge;
                }
                else if (!string.IsNullOrWhiteSpace(deleteId))
                {
                    partitionKey ??= deleteId;
                    var response = await container.DeleteItemAsync<dynamic>(deleteId, new PartitionKey(partitionKey));
                    result = (response.StatusCode == HttpStatusCode.NoContent) || (response.StatusCode == HttpStatusCode.OK);
                    statusCode = response.StatusCode;
                    requestCharge = response.RequestCharge;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (result)
            {
                Console.WriteLine(string.Format(Properties.Resources.LogMsgSuccess, statusCode));
            }
            else
            {
                Console.WriteLine(string.Format(Properties.Resources.LogMsgError, statusCode));
            }

            Console.WriteLine(string.Format(Properties.Resources.LogMsgRequestCharge, requestCharge));

            return result;
        }
    }
}
