namespace CosmosConsoleClient
{
    /// <summary>
    ///     Provides <see cref="string"/> consts for the <see cref="ItemCommand"/>.
    /// </summary>
    public static class ItemStrings
    {
        public const string Command = "item";

        public const string InitAll = "--init-all";
        public const string InitAllShort = "-i";

        public const string DatabaseId = "--database-id";
        public const string DatabaseIdShort = "--did";
        public const string DatabaseIdShortAlt = "--db";

        public const string ContainerId = "--container-id";
        public const string ContainerIdShort = "--cid";

        public const string PartitionPath = "--partition-path";

        public const string PartitionKey = "--partition-key";
        public const string PartitionKeyShort = "-p";

        public const string CreateItem = "--create-item";
        public const string CreateItemShort = "-c";

        public const string DeleteId = "--delete-id";
        public const string DeleteIdShort = "-d";

        public const string ListAll = "--list-all";
        public const string ListAllShort = "-l";

        public const string StartTime = "--start-time";
        public const string StartTimeShort = "--start";

        public const string StopTime = "--stop-time";
        public const string StopTimeShort = "--stop";

        public const string SqlQuery = "--sql-query";
        public const string SqlQueryShort = "-q";

        public const string Throughput = "--throughput";
        public const string ThroughputShort = "-t";
    }
}
