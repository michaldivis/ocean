using Data.Config;
using Data.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data.Contexts
{
    /// <summary>
    /// The SQLite implementation of <see cref="OceanDbContext"/>
    /// </summary>
    public class SqliteOceanDbContext : OceanDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (DbHelper.ConnectionStringInitialized)
            {
                optionsBuilder.UseSqlite(DbHelper.GetConnectionString().Construct());
            }
            else
            {
                Debug.WriteLine("[WARNING]: using migration database connection");
                optionsBuilder.UseSqlite(MigrationConstants.SqliteConnectionString);
            }
        }
    }
}
