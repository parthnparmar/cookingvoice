using Microsoft.EntityFrameworkCore;
using CookingWithVoice.Models;

namespace CookingWithVoice.Data
{
    public class CookingDbContext : DbContext
    {
        public CookingDbContext(DbContextOptions<CookingDbContext> options) : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Cuisine).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.VideoUrl).HasMaxLength(500);
                entity.Property(e => e.Difficulty).HasMaxLength(50);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.Property(e => e.Ingredients).HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
                entity.Property(e => e.Instructions).HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
            });
        }
    }
}