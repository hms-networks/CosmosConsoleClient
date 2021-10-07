using System;
using System.Net;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosConsoleClient
{
    /// <summary>
    ///     Command for container management and interaction.
    /// </summary>
    public static class ContainerCommand
    {
        /// <summary>
        ///     Specifies the command structure and handling.
        /// </summary>
        private static Command Command { get; } = new Command(
            name: ContainerStrings.Command,
            description: Properties.Resources.CmdDescContainer)
        {
            new Option(
                aliases: new string[] { ContainerStrings.InitDB, ContainerStrings.InitDBShort },
                description: Properties.Resources.ArgDescContainerInitDB,
                getDefaultValue: () => false,
                arity: ArgumentArity.Zero)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ContainerStrings.DatabaseId, ContainerStrings.DatabaseIdShort },
                description: Properties.Resources.ArgDescContainerDatabaseId,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = true
            },
            new Option(
                aliases: new string[] { ContainerStrings.PartitionPath, ContainerStrings.PartitionPathShort },
                description: Properties.Resources.ArgDescContainerPartitionPath,
                argumentType: typeof(string),
                getDefaultValue: () => "/id",
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ContainerStrings.CreateId, ContainerStrings.CreateIdShort },
                description: Properties.Resources.ArgDescContainerCreateId,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ContainerStrings.DeleteId, ContainerStrings.DeleteIdShort },
                description: Properties.Resources.ArgDescContainerDeleteId,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { ContainerStrings.Throughput, ContainerStrings.ThroughputShort },
                description: Properties.Resources.ArgDescContainerThroughput,
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

            Command.Handler = CommandHandler.Create<bool, string, string, string, string, int>(ExecuteCommandAsync);

            Command.AddValidator(cmd => {
                string result = null;
                bool createPresent = cmd.Children.Contains(ContainerStrings.CreateId) ||
                    cmd.Children.Contains(ContainerStrings.CreateIdShort);
                bool deletePresent = cmd.Children.Contains(ContainerStrings.DeleteId) ||
                    cmd.Children.Contains(ContainerStrings.DeleteIdShort);

                if (!(createPresent ^ deletePresent))
                {
                    result = string.Format(
                        Properties.Resources.ValidateErrMsgCreateXorDelete,
                        DatabaseStrings.CreateId,
                        DatabaseStrings.DeleteId);
                }

                return result;
            });

            root.AddCommand(Command);

            Initialized = true;
        }

        /// <summary>
        ///     The command execution logic.
        /// </summary>
        /// <param name="initDB">
        ///     When set <see langword="true"/>, the command will create the database if it does not exist.
        /// </param>
        /// <param name="databaseId">The ID of the database to interact with.</param>
        /// <param name="partitionPath">The partition key path to use for the container.</param>
        /// <param name="createId">The ID/name of the container to create.</param>
        /// <param name="deleteId">The ID/name of the container to delete.</param>
        /// <param name="throughput">The throughput (RU/sec).</param>
        /// <returns>
        ///     <see langword="true"/> on command execution success;
        ///     otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <exception cref="NotInitializedException"/>
        /// <exception cref="NotImplementedException"/>
        private static async Task<bool> ExecuteCommandAsync(
            bool initDB,
            string databaseId,
            string partitionPath,
            string createId,
            string deleteId,
            int throughput)
        {
            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            bool result;
            ContainerResponse response;
            string endpointUri = ConfigurationManager.AppSettings[AppStrings.EndPointUri];
            string primaryKey = ConfigurationManager.AppSettings[AppStrings.PrimaryKey];

            using (var client = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = AppStrings.AppName }))
            {
                Database db;

                if (initDB)
                {
                    db = await client.CreateDatabaseIfNotExistsAsync(databaseId);
                }
                else
                {
                    db = client.GetDatabase(databaseId);
                }

                if (!string.IsNullOrWhiteSpace(createId))
                {
                    response = await db.CreateContainerIfNotExistsAsync(createId, partitionPath, throughput);
                    result = (response.StatusCode == HttpStatusCode.Created) || (response.StatusCode == HttpStatusCode.OK);
                }
                else if (!string.IsNullOrWhiteSpace(deleteId))
                {
                    var container = db.GetContainer(deleteId);
                    response = await container.DeleteContainerAsync();
                    result = (response.StatusCode == HttpStatusCode.NoContent) || (response.StatusCode == HttpStatusCode.OK);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (result)
            {
                Console.WriteLine(string.Format(Properties.Resources.LogMsgSuccess, response.StatusCode));
            }
            else
            {
                Console.WriteLine(string.Format(Properties.Resources.LogMsgError, response.StatusCode));
            }

            Console.WriteLine(string.Format(Properties.Resources.LogMsgRequestCharge, response.RequestCharge));

            return result;
        }
    }
}
