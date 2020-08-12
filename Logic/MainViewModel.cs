using Data.Helpers;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Logic
{
    /// <summary>
    /// An example view model that holds some items and allows to add new items
    /// </summary>
    public class MainViewModel
    {
        public ObservableCollection<Fish> Fishes { get; set; } = new ObservableCollection<Fish>();

        public MainViewModel()
        {
            InitializeDatabase();
            LoadFishes();
        }

        /// <summary>
        /// Adds a new fish with random values to the <see cref="Fishes"/> collection and also saves it in the database
        /// </summary>
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

        /// <summary>
        /// Loads any previously created fish from the database to the <see cref="Fishes"/> collection
        /// </summary>
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

        /// <summary>
        /// Initializes the database
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Makes sure that the database exists</item>
        /// <item>Makes sure the database schema is up to date</item>
        /// </list>
        /// </remarks>
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
