namespace Data.Helpers
{
    /// <summary>
    /// The PostgreSQL implementation of <see cref="IConnectionString"/>
    /// </summary>
    public class PostgreSqlConnectionString : IConnectionString
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Construct()
        {
            return $"host={Host};port={Port};database={Database};user id={Username};password={Password};";
        }

        public DbProvider GetProvider()
        {
            return DbProvider.PostgreSql;
        }
    }
}
