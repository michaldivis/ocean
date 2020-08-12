using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts
{
    public class OceanDbContext : DbContext
    {
        public DbSet<Fish> Fishes { get; set; }
    }
}
