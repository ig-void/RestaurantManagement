using System;
using System.Threading.Tasks;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.Entities;

namespace RestaurantManagementSystem.Core.Domain.RepositoryContracts
{
    public interface ITableRepository
    {
        Task<Table?> GetByIdAsync(Guid id);
        Task<Table?> GetByNumberAndRestaurantAsync(Guid restaurantId, string tableNumber);
        Task<PaginatedList<Table>> GetPagedAsync(
            Guid? restaurantId,
            int? tableTypeId,
            string? status,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize);

        Task AddAsync(Table table);
        void Update(Table table);
        Task<bool> SaveChangesAsync();
    }
}
