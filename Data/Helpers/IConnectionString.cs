namespace Data.Helpers
{
    /// <summary>
    /// An abstraction of a connection string
    /// </summary>
    public interface IConnectionString
    {
        /// <summary>
        /// Constructs the connection string from properties
        /// </summary>
        string Construct();
        /// <summary>
        /// Returns the connection string's database provider
        /// </summary>
        DbProvider GetProvider();
    }
}
