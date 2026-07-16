using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;
using RestaurantManagementSystem.Core.Application.ServiceContracts;
using RestaurantManagementSystem.Infrastructure.Data;
using RestaurantManagementSystem.Infrastructure.Repository;
using RestaurantManagementSystem.Infrastructure.Services;

namespace RestaurantManagementSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                "Server=(localdb)\\mssqllocaldb;Database=RestaurantManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("RestaurantManagementSystem.Infrastructure")));

            // Repositories
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();

            // Services
            services.AddSingleton<IEmailService, MockEmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ILookupService, LookupService>();

            return services;
        }
    }
}
