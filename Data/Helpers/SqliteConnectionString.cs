namespace Data.Helpers
{
    public class SqliteConnectionString : IConnectionString
    {
        public string DbFilePath { get; set; }

        public string Construct()
        {
            return $"Data Source={DbFilePath};";
        }

        public DbProvider GetProvider()
        {
            return DbProvider.Sqlite;
        }
    }
}
