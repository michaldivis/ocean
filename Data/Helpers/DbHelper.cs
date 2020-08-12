using Data.Contexts;
using System;

namespace Data.Helpers
{
    /// <summary>
    /// Helper class for managing an EF Core database
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Holds the connection string and database provider information</item>
    /// <item>Provides and instance of the <see cref="OceanDbContext"/> once initialized</item>
    /// </list>
    /// </remarks>
    public static class DbHelper
    {
        private static bool _connectionStringInitialized;
        private static IConnectionString _connectionString = null;

        /// <summary>
        /// Indicates whether the <see cref="SetConnectionString(IConnectionString)"/> method has been called or not
        /// </summary>
        public static bool ConnectionStringInitialized => _connectionStringInitialized;

        /// <summary>
        /// Returns an instance of the <see cref="OceanDbContext"/> based on it's configuration intialized via the <seealso cref="SetConnectionString(IConnectionString)"/> method
        /// </summary>
        /// <returns></returns>
        public static OceanDbContext GetContext()
        {
            var provider = GetConnectionString().GetProvider();
            switch (provider)
            {
                case DbProvider.Sqlite:
                    return new SqliteOceanDbContext();
                case DbProvider.PostgreSql:
                    return new PostgreSqlOceanDbContext();
                default:
                    throw new Exception($"Invalid {nameof(DbProvider)} - {provider}");
            }
        }

        /// <summary>
        /// Initializes the helper with an implementation of <see cref="IConnectionString"/>
        /// </summary>
        /// <param name="cs">An implementation of <see cref="IConnectionString"/></param>
        public static void SetConnectionString(IConnectionString cs)
        {
            _connectionStringInitialized = true;
            _connectionString = cs;
        }

        /// <summary>
        /// Returns the current <see cref="IConnectionString"/> the helper has been initialized with the last via the <seealso cref="SetConnectionString(IConnectionString)"/> method
        /// </summary>
        public static IConnectionString GetConnectionString()
        {
            if (_connectionStringInitialized)
            {
                return _connectionString;
            }

            throw new Exception($"{nameof(_connectionString)} hasn't been initialized. Make sure to call {nameof(SetConnectionString)} before using the {nameof(GetConnectionString)}");
        }
    }
}
