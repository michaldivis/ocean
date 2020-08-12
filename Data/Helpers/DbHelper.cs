using System;

namespace Data.Helpers
{
    public static class DbHelper
    {
        private static bool _connectionStringInitialized;
        private static IConnectionString _connectionString = null;

        public static OceanDbContext GetContext()
        {
            return new OceanDbContext();
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
