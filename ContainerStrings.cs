namespace CosmosConsoleClient
{
    /// <summary>
    ///     Provides <see cref="string"/> consts for the <see cref="ContainerCommand"/>.
    /// </summary>
    public static class ContainerStrings
    {
        public const string Command = "container";

        public const string InitDB = "--init-db";
        public const string InitDBShort = "-i";

        public const string DatabaseId = "--database-id";
        public const string DatabaseIdShort = "--db";

        public const string PartitionPath = "--partition-path";
        public const string PartitionPathShort = "-p";

        public const string CreateId = "--create-id";
        public const string CreateIdShort = "-c";

        public const string DeleteId = "--delete-id";
        public const string DeleteIdShort = "-d";

        public const string Throughput = "--throughput";
        public const string ThroughputShort = "-t";
    }
}
