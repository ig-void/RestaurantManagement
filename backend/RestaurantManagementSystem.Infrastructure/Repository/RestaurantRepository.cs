using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.Entities;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;
using RestaurantManagementSystem.Infrastructure.Data;

namespace RestaurantManagementSystem.Infrastructure.Repository
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly AppDbContext _context;

        public RestaurantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Restaurant?> GetByIdAsync(Guid id)
        {
            return await _context.Restaurants
                .Include(r => r.CuisineType)
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Restaurant?> GetByNameAsync(string name)
        {
            return await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
        }

        public async Task<PaginatedList<Restaurant>> GetPagedAsync(
            int? cuisineTypeId,
            string? status,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Restaurants
                .Include(r => r.CuisineType)
                .Include(r => r.Tables)
                .AsQueryable();

            // Apply Filters
            if (cuisineTypeId.HasValue)
            {
                query = query.Where(r => r.CuisineTypeId == cuisineTypeId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(term) ||
                                         r.City.ToLower().Contains(term) ||
                                         r.Address.ToLower().Contains(term));
            }

            // Apply Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = isAscending ? query.OrderBy(r => r.Name) : query.OrderByDescending(r => r.Name);
                        break;
                    case "averagecost":
                    case "averagecostperperson":
                        query = isAscending ? query.OrderBy(r => r.AverageCostPerPerson) : query.OrderByDescending(r => r.AverageCostPerPerson);
                        break;
                    case "capacity":
                        query = isAscending ? query.OrderBy(r => r.Capacity) : query.OrderByDescending(r => r.Capacity);
                        break;
                    case "createddate":
                        query = isAscending ? query.OrderBy(r => r.CreatedDate) : query.OrderByDescending(r => r.CreatedDate);
                        break;
                    case "updateddate":
                        query = isAscending ? query.OrderBy(r => r.UpdatedDate) : query.OrderByDescending(r => r.UpdatedDate);
                        break;
                    default:
                        query = query.OrderBy(r => r.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(r => r.Name);
            }

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<Restaurant>(items, count, pageNumber, pageSize);
        }

        public async Task AddAsync(Restaurant restaurant)
        {
            await _context.Restaurants.AddAsync(restaurant);
        }

        public void Update(Restaurant restaurant)
        {
            _context.Restaurants.Update(restaurant);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
