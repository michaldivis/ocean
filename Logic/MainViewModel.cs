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
