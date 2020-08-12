﻿using Data.Helpers;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            ConfigureDatabase();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void ConfigureDatabase()
        {
            var dbPath = DependencyService.Get<IDbPathFinder>().GetFullPath("ocean.db");
            DbHelper.SetConnectionString(new SqliteConnectionString { DbFilePath = dbPath });
        }
    }
}
