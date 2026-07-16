using System;
using System.Threading.Tasks;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.Entities;

namespace RestaurantManagementSystem.Core.Domain.RepositoryContracts
{
    public interface IRestaurantRepository
    {
        Task<Restaurant?> GetByIdAsync(Guid id);
        Task<Restaurant?> GetByNameAsync(string name);
        Task<PaginatedList<Restaurant>> GetPagedAsync(
            int? cuisineTypeId,
            string? status,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize);
        
        Task AddAsync(Restaurant restaurant);
        void Update(Restaurant restaurant);
        Task<bool> SaveChangesAsync();
    }
}
