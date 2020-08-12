namespace Data.Helpers
{
    public interface IConnectionString
    {
        string Construct();
        DbProvider GetProvider();
    }
}
