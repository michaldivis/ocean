using Data.Contexts;
using System;

namespace Data.Helpers
{
    public static class DbHelper
    {
        private static bool _connectionStringInitialized;
        private static IConnectionString _connectionString = null;

        public static bool ConnectionStringInitialized => _connectionStringInitialized;

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

        public static void SetConnectionString(IConnectionString cs)
        {
            _connectionStringInitialized = true;
            _connectionString = cs;
        }

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
