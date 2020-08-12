using Data.Helpers;
using System.Windows;

namespace DesktopApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureDatabase();
        }

        private void ConfigureDatabase()
        {
            DbHelper.SetConnectionString(new PostgreSqlConnectionString
            {
                Host = "localhost",
                Port = "5434",
                Database = "ocean",
                Username = "postgres",
                Password = "d5vot8"
            });
        }
    }
}
