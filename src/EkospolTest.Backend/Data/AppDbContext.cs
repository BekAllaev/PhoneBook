using EkospolTest.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EkospolTest.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<PhoneNumber> PhoneNumbers => Set<PhoneNumber>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PhoneNumber>(entity =>
            {
                entity.ToTable("phone_numbers");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                    .HasColumnName("id");

                entity.Property(p => p.Number)
                    .HasColumnName("phone_number")
                    .HasMaxLength(32)
                    .IsRequired();

                entity.Property(p => p.IsPublic)
                    .HasColumnName("is_public")
                    .IsRequired();

                entity.Property(p => p.OwnerId)
                    .HasColumnName("owner_id")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(p => p.OwnerUsername)
                    .HasColumnName("owner_username")
                    .HasMaxLength(255)
                    .IsRequired();
            });
        }
    }
}
