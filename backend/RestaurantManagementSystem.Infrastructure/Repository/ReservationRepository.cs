using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.Entities;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;
using RestaurantManagementSystem.Infrastructure.Data;

namespace RestaurantManagementSystem.Infrastructure.Repository
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;
        private static readonly string[] ActiveStatuses = { "Pending", "Confirmed", "Checked-In" };

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation?> GetByIdAsync(Guid id)
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PaginatedList<Reservation>> GetPagedAsync(
            Guid? restaurantId,
            string? status,
            DateOnly? date,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .AsQueryable();

            // Apply Filters
            if (restaurantId.HasValue)
            {
                query = query.Where(r => r.RestaurantId == restaurantId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (date.HasValue)
            {
                query = query.Where(r => r.ReservationDate == date.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(r => (r.Customer != null && r.Customer.FullName.ToLower().Contains(term)) ||
                                         (r.Restaurant != null && r.Restaurant.Name.ToLower().Contains(term)) ||
                                         (r.Table != null && r.Table.TableNumber.ToLower().Contains(term)));
            }

            // Apply Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "customername":
                        query = isAscending ? query.OrderBy(r => r.Customer!.FullName) : query.OrderByDescending(r => r.Customer!.FullName);
                        break;
                    case "restaurantname":
                        query = isAscending ? query.OrderBy(r => r.Restaurant!.Name) : query.OrderByDescending(r => r.Restaurant!.Name);
                        break;
                    case "guestcount":
                        query = isAscending ? query.OrderBy(r => r.GuestCount) : query.OrderByDescending(r => r.GuestCount);
                        break;
                    case "reservationdate":
                        query = isAscending ? query.OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime) : query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime);
                        break;
                    case "status":
                        query = isAscending ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status);
                        break;
                    case "requestedon":
                        query = isAscending ? query.OrderBy(r => r.RequestedOn) : query.OrderByDescending(r => r.RequestedOn);
                        break;
                    default:
                        query = query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime);
            }

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<Reservation>(items, count, pageNumber, pageSize);
        }

        public async Task<PaginatedList<Reservation>> GetCustomerPagedAsync(
            Guid customerId,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Reservations
                .Include(r => r.Restaurant)
                .ThenInclude(res => res.CuisineType)
                .Include(r => r.Table)
                .ThenInclude(t => t.TableType)
                .Where(r => r.CustomerId == customerId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(r => r.Restaurant != null && r.Restaurant.Name.ToLower().Contains(term));
            }

            // Apply Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "restaurantname":
                        query = isAscending ? query.OrderBy(r => r.Restaurant!.Name) : query.OrderByDescending(r => r.Restaurant!.Name);
                        break;
                    case "reservationdate":
                        query = isAscending ? query.OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime) : query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime);
                        break;
                    case "status":
                        query = isAscending ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status);
                        break;
                    case "guestcount":
                        query = isAscending ? query.OrderBy(r => r.GuestCount) : query.OrderByDescending(r => r.GuestCount);
                        break;
                    default:
                        query = query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime);
            }

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<Reservation>(items, count, pageNumber, pageSize);
        }

        public async Task<int> GetActiveReservationsCountForCustomerAsync(Guid customerId)
        {
            return await _context.Reservations
                .CountAsync(r => r.CustomerId == customerId && ActiveStatuses.Contains(r.Status));
        }

        public async Task<bool> HasDuplicateActiveReservationAsync(Guid customerId, Guid restaurantId, DateOnly date)
        {
            return await _context.Reservations
                .AnyAsync(r => r.CustomerId == customerId &&
                               r.RestaurantId == restaurantId &&
                               r.ReservationDate == date &&
                               ActiveStatuses.Contains(r.Status));
        }

        public async Task<bool> HasSuitableTableAsync(Guid restaurantId, int guestCount)
        {
            return await _context.Tables
                .AnyAsync(t => t.RestaurantId == restaurantId &&
                               t.SeatingCapacity >= guestCount &&
                               t.Status != "Maintenance" &&
                               !t.IsDeleted);
        }

        public async Task<Table?> GetBestMatchingAvailableTableAsync(Guid restaurantId, int guestCount)
        {
            return await _context.Tables
                .Where(t => t.RestaurantId == restaurantId &&
                            t.Status == "Available" &&
                            t.SeatingCapacity >= guestCount &&
                            !t.IsDeleted)
                .OrderBy(t => t.SeatingCapacity)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
        }

        public void Update(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
