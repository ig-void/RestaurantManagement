using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.Entities;

namespace RestaurantManagementSystem.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<CuisineType> CuisineTypes { get; set; }
        public DbSet<TableType> TableTypes { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure custom User properties
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.City).IsRequired().HasMaxLength(50);
            });

            // CuisineType configuration
            modelBuilder.Entity<CuisineType>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Name).IsUnique();
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            // TableType configuration
            modelBuilder.Entity<TableType>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            });

            // Restaurant configuration
            modelBuilder.Entity<Restaurant>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Address).IsRequired().HasMaxLength(250);
                entity.Property(r => r.City).IsRequired().HasMaxLength(50);
                entity.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(r => r.AverageCostPerPerson).HasPrecision(18, 2);
                entity.Property(r => r.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(r => r.CuisineType)
                      .WithMany(c => c.Restaurants)
                      .HasForeignKey(r => r.CuisineTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(r => !r.IsDeleted);
            });

            // Table configuration
            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => new { t.RestaurantId, t.TableNumber }).IsUnique();
                entity.Property(t => t.TableNumber).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Status).IsRequired().HasMaxLength(50);

                entity.HasOne(t => t.Restaurant)
                      .WithMany(r => r.Tables)
                      .HasForeignKey(t => t.RestaurantId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.TableType)
                      .WithMany(tt => tt.Tables)
                      .HasForeignKey(t => t.TableTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(t => !t.IsDeleted);
            });

            // Reservation configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Status).IsRequired().HasMaxLength(50);
                entity.Property(r => r.SpecialRequests).HasMaxLength(500);

                entity.HasOne(r => r.Customer)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(r => r.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Restaurant)
                      .WithMany(res => res.Reservations)
                      .HasForeignKey(r => r.RestaurantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Table)
                      .WithMany(t => t.Reservations)
                      .HasForeignKey(r => r.TableId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserId = Guid.Empty;
            if (Guid.TryParse(userIdString, out var parsedId))
            {
                currentUserId = parsedId;
            }

            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity.CreatedBy == Guid.Empty && currentUserId != Guid.Empty)
                        {
                            entry.Entity.CreatedBy = currentUserId;
                        }
                        if (entry.Entity.CreatedDate == default)
                        {
                            entry.Entity.CreatedDate = DateTime.UtcNow;
                        }
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedBy = currentUserId;
                        entry.Entity.UpdatedDate = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        // Intercept hard delete and convert to soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedBy = currentUserId;
                        entry.Entity.DeletedDate = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}
