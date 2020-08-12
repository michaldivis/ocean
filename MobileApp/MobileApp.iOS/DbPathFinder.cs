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