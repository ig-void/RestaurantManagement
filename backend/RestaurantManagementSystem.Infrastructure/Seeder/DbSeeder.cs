using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagementSystem.Core.Domain.Entities;
using RestaurantManagementSystem.Infrastructure.Data;

namespace RestaurantManagementSystem.Infrastructure.Seeder
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            // 1. Run migrations automatically
            await context.Database.MigrateAsync();

            // 2. Seed Roles
            var roles = new[] { "RestaurantManager", "Customer" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName, NormalizedName = roleName.ToUpper() });
                }
            }

            // 3. Seed Default User (RestaurantManager role)
            var managerEmail = "manager@restaurant.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            if (managerUser == null)
            {
                managerUser = new User
                {
                    Id = Guid.Parse("a8661858-a83d-4c31-8977-505ad42c55ab"),
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "Admin Manager",
                    PhoneNumber = "1234567890",
                    City = "New York",
                    EmailConfirmed = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(managerUser, "Manager@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "RestaurantManager");
                }
                else
                {
                    throw new Exception($"Failed to seed default manager: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // 4. Seed Cuisine Types
            if (!await context.CuisineTypes.AnyAsync())
            {
                context.CuisineTypes.AddRange(
                    new CuisineType { Name = "Italian" },
                    new CuisineType { Name = "Chinese" },
                    new CuisineType { Name = "Indian" },
                    new CuisineType { Name = "Seafood" },
                    new CuisineType { Name = "Continental" }
                );
                await context.SaveChangesAsync();
            }

            // 5. Seed Table Types
            if (!await context.TableTypes.AnyAsync())
            {
                context.TableTypes.AddRange(
                    new TableType { Name = "Seating 2 (Couple)", Capacity = 2 },
                    new TableType { Name = "Seating 4 (Family)", Capacity = 4 },
                    new TableType { Name = "Seating 6 (Large Family)", Capacity = 6 },
                    new TableType { Name = "Seating 8+ (Banquet)", Capacity = 8 }
                );
                await context.SaveChangesAsync();
            }

            // 6. Seed Restaurants
            if (!await context.Restaurants.AnyAsync())
            {
                var cuisines = await context.CuisineTypes.ToListAsync();
                var indianCuisine = cuisines.FirstOrDefault(c => c.Name == "Indian")?.Id ?? 3;
                var italianCuisine = cuisines.FirstOrDefault(c => c.Name == "Italian")?.Id ?? 1;
                var seafoodCuisine = cuisines.FirstOrDefault(c => c.Name == "Seafood")?.Id ?? 4;
                var chineseCuisine = cuisines.FirstOrDefault(c => c.Name == "Chinese")?.Id ?? 2;
                var continentalCuisine = cuisines.FirstOrDefault(c => c.Name == "Continental")?.Id ?? 5;

                context.Restaurants.AddRange(
                    new Restaurant
                    {
                        Id = Guid.Parse("01c402ad-a841-4775-9c96-6d63fa2c030d"),
                        Name = "Spice Haven",
                        CuisineTypeId = indianCuisine,
                        Address = "123 Curry Street",
                        City = "New York",
                        PhoneNumber = "555-0101",
                        OpeningTime = new TimeOnly(11, 0),
                        ClosingTime = new TimeOnly(23, 0),
                        AverageCostPerPerson = 25.00m,
                        Capacity = 40,
                        Status = "Active",
                        CreatedBy = managerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Id = Guid.Parse("c1d582af-5847-4952-b883-7d88c03e8ff0"),
                        Name = "The Italian Kitchen",
                        CuisineTypeId = italianCuisine,
                        Address = "456 Pasta Avenue",
                        City = "New York",
                        PhoneNumber = "555-0102",
                        OpeningTime = new TimeOnly(12, 0),
                        ClosingTime = new TimeOnly(22, 30),
                        AverageCostPerPerson = 35.00m,
                        Capacity = 30,
                        Status = "Active",
                        CreatedBy = managerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Id = Guid.Parse("e30017a6-2394-4d89-9430-863fd3a31bf9"),
                        Name = "Oceanview Seafood",
                        CuisineTypeId = seafoodCuisine,
                        Address = "789 Marina Blvd",
                        City = "Miami",
                        PhoneNumber = "555-0103",
                        OpeningTime = new TimeOnly(11, 30),
                        ClosingTime = new TimeOnly(22, 0),
                        AverageCostPerPerson = 45.00m,
                        Capacity = 50,
                        Status = "Active",
                        CreatedBy = managerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Id = Guid.Parse("7a409fbe-c8ff-45a7-ab0c-4be5016e788b"),
                        Name = "Dragon Palace",
                        CuisineTypeId = chineseCuisine,
                        Address = "101 Wok Road",
                        City = "San Francisco",
                        PhoneNumber = "555-0104",
                        OpeningTime = new TimeOnly(11, 0),
                        ClosingTime = new TimeOnly(22, 0),
                        AverageCostPerPerson = 20.00m,
                        Capacity = 60,
                        Status = "Active",
                        CreatedBy = managerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Id = Guid.Parse("db802611-37ea-41ea-ad44-63be5f8e5f5f"),
                        Name = "Garden Bistro",
                        CuisineTypeId = continentalCuisine,
                        Address = "202 Patio Lane",
                        City = "Chicago",
                        PhoneNumber = "555-0105",
                        OpeningTime = new TimeOnly(8, 0),
                        ClosingTime = new TimeOnly(21, 0),
                        AverageCostPerPerson = 18.00m,
                        Capacity = 25,
                        Status = "Active",
                        CreatedBy = managerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
