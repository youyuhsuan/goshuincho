using Microsoft.EntityFrameworkCore;
using backend.Models;


// Database context for the application, managing entity operations and database connections
namespace backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // Database table mappings - Entity Framework will create corresponding tables
        public DbSet<User> Users => Set<User>();
    }
}