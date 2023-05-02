using Location.Domain.Entities;
using Location.Models;
using Microsoft.EntityFrameworkCore;
namespace Location.Domain
{
    public class LocationDbContext : DbContext
    {
        public LocationDbContext()
        {

        }
        public LocationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<LocationEntity> Location { get; set; }
    }
}


