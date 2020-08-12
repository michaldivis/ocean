using Data.Helpers;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Data
{
    public class OceanDbContext : DbContext
    {
        public DbSet<Fish> Fishes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #region Migration code

            //var migrationCs = new SqliteConnectionString()
            //{
            //    DbFilePath = "nothing.db"
            //};
            //optionsBuilder.UseSqlite(migrationCs.Construct());
            var migrationCs = new PostgreSqlConnectionString
            {
                Host = "localhost",
                Port = "5434",
                Database = "ocean",
                Username = "postgres",
                Password = "d5vot8"
            };
            optionsBuilder.UseNpgsql(migrationCs.Construct());

            return;

            #endregion

            if (!optionsBuilder.IsConfigured)
            {
                var cs = DbHelper.GetConnectionString();

                switch (cs.GetProvider())
                {
                    case DbProvider.Sqlite:
                        optionsBuilder.UseSqlite(cs.Construct());
                        break;
                    case DbProvider.PostgreSql:
                        optionsBuilder.UseNpgsql(cs.Construct());
                        break;
                    default:
                        throw new NullReferenceException($"Invalid database provider > {cs.GetProvider()}");
                }
            }
        }
    }
}
