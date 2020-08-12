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