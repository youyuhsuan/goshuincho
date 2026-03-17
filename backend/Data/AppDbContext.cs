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
        public DbSet<TokenBlacklist> TokenBlacklists => Set<TokenBlacklist>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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
                    .IsRequired(false)
                    .HasMaxLength(500);

                entity.Property(e => e.Picture)
                    .IsRequired(false)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                 .IsRequired()
                 .HasDefaultValueSql("GETUTCDATE()");

                // Index
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.GoogleId)
                    .IsUnique()
                    .HasFilter("[GoogleId] IS NOT NULL");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TokenHash)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.RevokedAt)
                    .IsRequired(false);

                entity.Property(e => e.Reason)
                    .IsRequired(false)
                    .HasMaxLength(50);

                // FK
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<TokenBlacklist>(entity =>
                 {
                     entity.ToTable("TokenBlacklists");
                     entity.HasKey(e => e.Id);

                     entity.Property(e => e.Id)
                         .HasDefaultValueSql("NEWID()")
                         .ValueGeneratedOnAdd();

                     entity.Property(e => e.Jti).HasMaxLength(100);

                     // Index
                     entity.HasIndex(e => e.Jti).IsUnique();
                     entity.HasIndex(rt => rt.UserId);
                     entity.HasIndex(rt => rt.ExpiresAt);
                 });

            base.OnModelCreating(modelBuilder);
        }
    }
}