using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


// Database context for the application, managing entity operations and database connections
namespace backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // Database table mappings - Entity Framework will create corresponding tables
        public DbSet<User> Users => Set<User>();
        public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.GoogleId)
                    .IsRequired(false)
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Picture)
                    .IsRequired(false)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                 .IsRequired()
                 .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.GoogleId)
                    .IsUnique()
                    .HasFilter("[GoogleId] IS NOT NULL");
            });


            modelBuilder.Entity<RevokedToken>(entity =>
                 {
                     entity.HasIndex(rt => rt.Jti).IsUnique();
                     entity.HasIndex(rt => rt.UserId);
                     entity.HasIndex(rt => rt.ExpiresAt);
                 });

            base.OnModelCreating(modelBuilder);
        }
    }
}