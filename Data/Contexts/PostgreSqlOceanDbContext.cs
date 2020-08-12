using Data.Config;
using Data.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data.Contexts
{
    public class PostgreSqlOceanDbContext : OceanDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (DbHelper.ConnectionStringInitialized)
            {
                optionsBuilder.UseNpgsql(DbHelper.GetConnectionString().Construct());
            }
            else
            {
                Debug.WriteLine("[WARNING]: using migration database connection");
                optionsBuilder.UseNpgsql(MigrationConstants.PostgreSqlConnectionString);
            }
        }
    }
}
