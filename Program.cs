using System.CommandLine;

namespace CosmosConsoleClient
{
    /// <summary>
    ///     Command line application to assist with interaction with a Cosmos DB instance.
    ///     The current version of "Microsoft.Azure.Cosmos" is only compatible with .NET Core 2.0 SDK, thus
    ///     this program acts as an abstraction layer which can be interfaced with newer versions of .NET Core.
    /// </summary>
    class Program
    {
        /// <summary>
        ///     Main entry point of command line application.
        /// </summary>
        /// <param name="args">
        ///     Command line arguments. These arguments will be passed into the System.Commandline
        ///     RootCommand object. Which command is invoked is determined by this object's internal
        ///     pattern matching logic. See the registered commands for details on each command's
        ///     syntax.
        /// </param>
        /// <returns>
        ///     0 on success; otherwise, !0.
        /// </returns>
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand(Properties.Resources.CmdDescRoot);

            // Initialize the program's commands and add to the root command object.
            DatabaseCommand.Register(rootCommand);
            ContainerCommand.Register(rootCommand);
            ItemCommand.Register(rootCommand);

            rootCommand.TreatUnmatchedTokensAsErrors = true;

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
