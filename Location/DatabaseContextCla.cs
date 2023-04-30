using Location.Models;
using Microsoft.EntityFrameworkCore;
namespace Location
{
    public class DatabaseContextCla : DbContext
    {
        public DatabaseContextCla()
        {

        }
        public DatabaseContextCla(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<LocationModel> Location { get; set; }
    }
}  


