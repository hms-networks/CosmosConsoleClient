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
    ///     Command for database management and interaction.
    /// </summary>
    public static class DatabaseCommand
    {
        /// <summary>
        ///     Specifies the command structure and handling.
        /// </summary>
        private static Command Command { get; } = new Command(
            name: DatabaseStrings.Command,
            description: Properties.Resources.CmdDescDatabase)
        {
            new Option(
                aliases: new string[] { DatabaseStrings.CreateId, DatabaseStrings.CreateIdShort },
                description: Properties.Resources.ArgDescDatabaseCreateId,
                argumentType: typeof(string),
                getDefaultValue: null,
                arity: ArgumentArity.ExactlyOne)
            {
                IsRequired = false
            },
            new Option(
                aliases: new string[] { DatabaseStrings.DeleteId, DatabaseStrings.DeleteIdShort },
                description: Properties.Resources.ArgDescDatabaseDeleteId,
                argumentType: typeof(string),
                getDefaultValue: null,
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

            Command.Handler = CommandHandler.Create<string, string>(ExecuteCommandAsync);

            Command.AddValidator(cmd => {
                string result = null;
                bool createPresent = cmd.Children.Contains(DatabaseStrings.CreateId) ||
                    cmd.Children.Contains(DatabaseStrings.CreateIdShort);
                bool deletePresent = cmd.Children.Contains(DatabaseStrings.DeleteId) ||
                    cmd.Children.Contains(DatabaseStrings.DeleteIdShort);

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
        /// <param name="createId">The ID/name of the database to create.</param>
        /// <param name="deleteId">The ID/name of the database to delete.</param>
        /// <returns>
        ///     <see langword="true"/> on command execution success;
        ///     otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <exception cref="NotInitializedException"/>
        /// <exception cref="NotImplementedException"/>
        private static async Task<bool> ExecuteCommandAsync(
            string createId,
            string deleteId)
        {
            if (!Initialized)
            {
                throw new NotInitializedException();
            }

            bool result;
            DatabaseResponse response;
            string endpointUri = ConfigurationManager.AppSettings[AppStrings.EndPointUri];
            string primaryKey = ConfigurationManager.AppSettings[AppStrings.PrimaryKey];

            using (var client = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = AppStrings.AppName }))
            {
                if (!string.IsNullOrWhiteSpace(createId))
                {
                    response = await client.CreateDatabaseIfNotExistsAsync(createId);
                    result = (response.StatusCode == HttpStatusCode.Created) || (response.StatusCode == HttpStatusCode.OK);
                }
                else if (!string.IsNullOrWhiteSpace(deleteId))
                {
                    var db = client.GetDatabase(deleteId);
                    response = await db.DeleteAsync();
                    result = (response.StatusCode == HttpStatusCode.NoContent) || (response.StatusCode == HttpStatusCode.OK);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (result)
            {
                Console.WriteLine(Properties.Resources.LogMsgSuccess, response.StatusCode);
            }
            else
            {
                Console.WriteLine(Properties.Resources.LogMsgError, response.StatusCode);
            }

            Console.WriteLine(Properties.Resources.LogMsgRequestCharge, response.RequestCharge);

            return result;
        }
    }
}
