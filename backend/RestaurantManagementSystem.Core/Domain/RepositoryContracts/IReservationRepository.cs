using System;
using System.Threading.Tasks;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.Entities;

namespace RestaurantManagementSystem.Core.Domain.RepositoryContracts
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(Guid id);
        
        Task<PaginatedList<Reservation>> GetPagedAsync(
            Guid? restaurantId,
            string? status,
            DateOnly? date,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize);

        Task<PaginatedList<Reservation>> GetCustomerPagedAsync(
            Guid customerId,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize);

        Task<int> GetActiveReservationsCountForCustomerAsync(Guid customerId);
        Task<bool> HasDuplicateActiveReservationAsync(Guid customerId, Guid restaurantId, DateOnly date);
        Task<bool> HasSuitableTableAsync(Guid restaurantId, int guestCount);
        Task<Table?> GetBestMatchingAvailableTableAsync(Guid restaurantId, int guestCount);
        
        Task AddAsync(Reservation reservation);
        void Update(Reservation reservation);
        Task<bool> SaveChangesAsync();
    }
}
