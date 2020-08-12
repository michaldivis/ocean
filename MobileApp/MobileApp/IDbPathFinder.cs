namespace MobileApp
{
    /// <summary>
    /// An abstraction of a something that returns a valid path on the device where a database file can be saved
    /// </summary>
    public interface IDbPathFinder
    {
        /// <summary>
        /// Returns a valid path on the device where a database file can be saved, naming the file according to the <paramref name="name"/> parameter
        /// </summary>
        /// <param name="name">Name of the database file</param>
        string GetFullPath(string name);
    }
}
