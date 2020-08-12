using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts
{
    /// <summary>
    /// The base database context class
    /// </summary>
    /// <remarks>
    /// Holds the <see cref="DbSet{T}"/> properties
    /// </remarks>
    public class OceanDbContext : DbContext
    {
        public DbSet<Fish> Fishes { get; set; }
    }
}
