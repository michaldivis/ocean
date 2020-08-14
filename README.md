*I could never find a good example of using a single EntityFramework Core project in both WPF and Xamarin.Forms apps, with multiple providers. In theory, it's dead simple, but there are a few little things you have to tinker with to make it work. Here's how...*

### The basic idea
The way I go about this is I create a basic database context that only contains the `DbSet` properties and then make one more context per database provider (PostgreSQL, SQLite in this case) that derives from the basic one. The derived contexts then override their `OnConfiguring` methods where they set up the connection for their specific provider using a connection string extracted from a helper class.

Why multiple database contexts?
In short: migrations. We want to be able to perform migrations for each provider separately since each provider might need the migration to be a bit different from the others. Having multiple contexts allows us to call the `Add-Migration` command on each of the contexts separately.

# Solution structure
## Data (.NET Standard class library) 
This project will contain the EF Core database context, data models, and some helpers.
### Dependencies
- Nuget
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Design
  - Microsoft.EntityFrameworkCore.Tools
  - Npgsql.EntityFrameworkCore.PostgreSQL
  - Microsoft.EntityFrameworkCore.Sqlite

## Logic (.NET Standard class library)
This project will contain a simple view model that will be shared by the other apps. This will be referencing the Data project.
### Dependencies
- Projects
  - Data
- Nuget
  - Microsoft.EntityFrameworkCore

## DesktopApp (.NET Core WPF)
An example WPF app that will use PostgreSQL as the database. This will be referencing the Data and Logic projects.
### Dependencies
- Projects
  - Data
  - Logic
- Nuget
  - Microsoft.EntityFrameworkCore
  - Npgsql.EntityFrameworkCore.PostgreSQL

## MobileApp (Xamarin.Forms)
An example mobile app that will use SQLite as the database. This will be referencing the Data and Logic projects.
### Dependencies
- Projects
  - Data
  - Logic
- Nuget
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Sqlite

# Used versions

## SDK
- .NET Core 3.1
- .NET Standard 2.0
- Xamarin.Forms 4.8

## Nuget packages
- Microsoft.EntityFrameworkCore 3.1.7
- Npgsql.EntityFrameworkCore.PostgreSQL 3.1.4
- Microsoft.EntityFrameworkCore.Sqlite 3.1.7

# Creating the data project - contexts, models, helpers

## Folders
I've created two folders in the data project
- Config - migration configuration values will be here
- Contexts - db contexts for different platforms will be here
- Helpers - helper classes for making this usable on multiple platforms will be here
- Migrations
    - PostgreSqlMigrations - Migrations for PostgreSQL will be here
    - SqliteMigrations - Migrations for SQLite will be here
- Models - data model classes will be here

## Models
Let's say we want to have a database of fish. Here's a class called `Fish`.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    public class Fish
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
    }
}
```
I'm going to using this class as the example data model.

## A way to use different database contexts for different platforms

Since we want to use different databases (PostgreSQL, SQLite) for the two apps, we'll need a way to make the Data project take in some configuration (wanted database provider, connection string).

For that I've created the following items in the "Helpers" folder:

### DbProvider.cs
This is an enum that will help us determine what database provider to use
```csharp
namespace Data.Helpers
{
    public enum DbProvider
    {
        Sqlite,
        PostgreSql
    }
}
```

### IConnectionString.cs
An abstraction of a connection string, which differs from one provider to another
```csharp
namespace Data.Helpers
{
    public interface IConnectionString
    {
        string Construct();
        DbProvider GetProvider();
    }
}
```

### PostgreSqlConnectionString.cs
A connection string definition for PostgreSQL
```csharp
namespace Data.Helpers
{
    public class PostgreSqlConnectionString : IConnectionString
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Construct()
        {
            return $"host={Host};port={Port};database={Database};user id={Username};password={Password};";
        }

        public DbProvider GetProvider()
        {
            return DbProvider.PostgreSql;
        }
    }
}
```

### SqliteConnectionString.cs
A connection string definition for SQLite
```csharp
namespace Data.Helpers
{
    public class SqliteConnectionString : IConnectionString
    {
        public string DbFilePath { get; set; }

        public string Construct()
        {
            return $"Data Source={DbFilePath};";
        }

        public DbProvider GetProvider()
        {
            return DbProvider.Sqlite;
        }
    }
}
```

### DbHelper.cs
This static class is going to be used to configure the connection from the other projects (DesktopApp, MobileApp)
```csharp
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
```

## Database contexts

To use this on multiple platforms, we need to have different database contexts for each platform. However, they need to share the same data sets.

That's why we'll create one base context and derive the others from that.

Here's the base context.

```csharp
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts
{
    public class OceanDbContext : DbContext
    {
        public DbSet<Fish> Fishes { get; set; }
    }
}
```

Here's the PostgreSQL context:

```csharp
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
```

Here's the SQLite context:

```csharp
using Data.Config;
using Data.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data.Contexts
{
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
```

## Almost ready, let's migrate

Now before we use the app, there's one last step - migrations.

To create an initial (or any other) migration for each database context, we'll to do the following:

### Change the Data project's target framework to multiple targets to run migrations with it

Go to the Data.csproj and change this:
```
<TargetFramework>netstandard2.0</TargetFramework>
```

to this

```
<TargetFrameworks>netcoreapp2.0;netstandard2.0</TargetFrameworks>
```

The reason we do that is to be able to run commands using the Data project in the Package Manager Console (which needs a runnable project type to function).

### Migrate

Open the Package Manager Console (Tools -> Nuget Package Manager -> Package Manager Console in Visual Studio 2019)

Make sure to have the Data project selected as the default project.

And run the migration command:

#### SQLite migration command
`Add-Migration SqliteMigration001 -Context SqliteOceanDbContext -OutputDir Migrations/SqliteMigrations`

#### PostgreSQL migration command
`Add-Migration PostgreSqlMigration001 -Context PostgreSqlOceanDbContext -OutputDir Migrations/PostgreSqlMigrations`

After the command finishes, there should be some new files in the corresponding Migrations/ folder.

We're now ready to use the thing!

# Configuring the connection for each platform

## MobileApp project

Now let's configure the database from the mobile project.

Let's first add a reference to the Data project.

### Database file path

We'll need a way to get some valid database file path for each platform (Android, iOS). Let's do that.

#### Create an interface called `IDbPathFinder` in the MobileApp project

```csharp
namespace MobileApp
{
    public interface IDbPathFinder
    {
        string GetFullPath(string name);
    }
}
```

#### Create an Android implementation of that interface in the MobileApp.Android project

```csharp
using MobileApp.Droid;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(DbPathFinder))]
namespace MobileApp.Droid
{
    public class DbPathFinder : IDbPathFinder
    {
        public string GetFullPath(string name)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), name);
        }
    }
}
```

#### Create an iOS implementation of that interface in the MobileApp.iOS project

```csharp
using MobileApp.iOS;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(DbPathFinder))]
namespace MobileApp.iOS
{
    public class DbPathFinder : IDbPathFinder
    {
        public string GetFullPath(string name)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", name);
        }
    }
}
```

### Configuring on start

To configure the database connection before we need to use, we'll make all the call in the `App.xaml.cs` file.

We'll add a method called `ConfigureDatabase`, that will contain the configuration and then call that method from the `OnStart` method.

The App.xaml.cs file should now look like this:

```csharp
using Data.Helpers;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            ConfigureDatabase();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
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
```

## DesktopApp project

Now let's configure the database from the desktop project.

Let's first add a reference to the Data project.

To configure the database connection before we need to use, we'll make all the call in the `App.xaml.cs` file.

Add a method called `ConfigureDatabase`, that will contain the configration.

Then override the `OnStartup` method and call the `ConfigureDatabase` from it.

The App.xaml.cs file should now look like this:

```csharp
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
                Password = "postgres"
            });
        }
    }
}
```

Note that in this example I'm hard coding the connection string, which is a terrible idea. Don't do that!



# Let's try it

## Basic view model

To try this, I'll create a simple class called `MainViewModel` that will serve as a view model for both the DesktopApp and the MobileApp.

I've referenced the Data project and created the following class:
```csharp
using Data.Helpers;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Logic
{
    public class MainViewModel
    {
        public ObservableCollection<Fish> Fishes { get; set; } = new ObservableCollection<Fish>();

        public MainViewModel()
        {
            InitializeDatabase();
            LoadFishes();
        }

        public void AddRandomFish()
        {
            var randy = new Random();

            using (var db = DbHelper.GetContext())
            {
                var newFish = new Fish
                {
                    Name = new[] { "Shark", "Blue whale", "Nemo" }[randy.Next(3)],
                    Length = randy.Next(1, 15)
                };
                db.Fishes.Add(newFish);
                db.SaveChanges();
                Fishes.Add(newFish);
            }
        }

        public void LoadFishes()
        {
            using (var db = DbHelper.GetContext())
            {
                var fishes = db.Fishes.ToList();
                foreach (var fish in fishes)
                {
                    Fishes.Add(fish);
                }
            }
        }

        public void InitializeDatabase()
        {
            try
            {
                using (var db = DbHelper.GetContext())
                {
                    db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Oops! The database could not be initialized. " + ex.ToString());
            }
        }
    }
}
```

## Desktop app view

I've added a reference to the Logic project and edited the MainWindow to look like this:

MainWindow.xaml.cs
```csharp
using Logic;
using System.Windows;

namespace DesktopApp
{
    public partial class MainWindow : Window
    {
        private MainViewModel _model;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _model = new MainViewModel();
        }

        private void BtnAddFish_Click(object sender, RoutedEventArgs e)
        {
            _model.AddRandomFish();
        }
    }
}
```

MainWindow.xaml
```xaml
<Window
    x:Class="DesktopApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:local="clr-namespace:DesktopApp"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    Title="Fish list"
    Width="800"
    Height="450"
    WindowStartupLocation="CenterScreen"
    mc:Ignorable="d">
    <Grid>
        <ListView ItemsSource="{Binding Fishes}">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <TextBlock>
                        <Run Text="{Binding Name}" />
                        <Run Text="-" />
                        <Run Text="{Binding Length, StringFormat='length: {0} meters'}" />
                    </TextBlock>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
        <Button
            Margin="10"
            Padding="10"
            HorizontalAlignment="Right"
            VerticalAlignment="Bottom"
            Click="BtnAddFish_Click"
            Content="Add random fish" />
    </Grid>
</Window>
```

## Mobile app view

I've added a reference to the Logic project and edited the MainPage to look like this:

MainPage.xaml.cs
```csharp
using Logic;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _model;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = _model = new MainViewModel();
        }

        private void BtnAddRandomFish_Clicked(object sender, System.EventArgs e)
        {
            _model.AddRandomFish();
        }
    }
}
```

MainPage.xaml
```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="MobileApp.MainPage"
    xmlns="http://xamarin.com/schemas/2014/forms"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <Grid>
        <CollectionView ItemsSource="{Binding Fishes}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Padding="10,5,10,5" HasShadow="False">
                        <Label>
                            <Label.FormattedText>
                                <FormattedString>
                                    <Span Text="{Binding Name}" />
                                    <Span Text=" - " />
                                    <Span Text="{Binding Length, StringFormat='length: {0} meters'}" />
                                </FormattedString>
                            </Label.FormattedText>
                        </Label>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <Button
            Margin="10"
            Clicked="BtnAddRandomFish_Clicked"
            HorizontalOptions="End"
            Text="Add random fish"
            VerticalOptions="End" />
    </Grid>

</ContentPage>
```

# That's it
Woohoo! That's it.
