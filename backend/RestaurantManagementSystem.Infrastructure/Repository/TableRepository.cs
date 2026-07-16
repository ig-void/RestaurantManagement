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
    public class TableRepository : ITableRepository
    {
        private readonly AppDbContext _context;

        public TableRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Table?> GetByIdAsync(Guid id)
        {
            return await _context.Tables
                .Include(t => t.Restaurant)
                .Include(t => t.TableType)
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Table?> GetByNumberAndRestaurantAsync(Guid restaurantId, string tableNumber)
        {
            return await _context.Tables
                .FirstOrDefaultAsync(t => t.RestaurantId == restaurantId && t.TableNumber.ToLower() == tableNumber.ToLower());
        }

        public async Task<PaginatedList<Table>> GetPagedAsync(
            Guid? restaurantId,
            int? tableTypeId,
            string? status,
            string? searchTerm,
            string? sortBy,
            bool isAscending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Tables
                .Include(t => t.Restaurant)
                .Include(t => t.TableType)
                .AsQueryable();

            // Apply Filters
            if (restaurantId.HasValue)
            {
                query = query.Where(t => t.RestaurantId == restaurantId.Value);
            }

            if (tableTypeId.HasValue)
            {
                query = query.Where(t => t.TableTypeId == tableTypeId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(t => t.TableNumber.ToLower().Contains(term) ||
                                         (t.Restaurant != null && t.Restaurant.Name.ToLower().Contains(term)));
            }

            // Apply Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "tablenumber":
                        query = isAscending ? query.OrderBy(t => t.TableNumber) : query.OrderByDescending(t => t.TableNumber);
                        break;
                    case "seatingcapacity":
                        query = isAscending ? query.OrderBy(t => t.SeatingCapacity) : query.OrderByDescending(t => t.SeatingCapacity);
                        break;
                    case "status":
                        query = isAscending ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status);
                        break;
                    case "restaurant":
                    case "restaurantname":
                        query = isAscending ? query.OrderBy(t => t.Restaurant!.Name) : query.OrderByDescending(t => t.Restaurant!.Name);
                        break;
                    case "createddate":
                        query = isAscending ? query.OrderBy(t => t.CreatedDate) : query.OrderByDescending(t => t.CreatedDate);
                        break;
                    case "updateddate":
                        query = isAscending ? query.OrderBy(t => t.UpdatedDate) : query.OrderByDescending(t => t.UpdatedDate);
                        break;
                    default:
                        query = query.OrderBy(t => t.Restaurant!.Name).ThenBy(t => t.TableNumber);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(t => t.Restaurant!.Name).ThenBy(t => t.TableNumber);
            }

            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<Table>(items, count, pageNumber, pageSize);
        }

        public async Task AddAsync(Table table)
        {
            await _context.Tables.AddAsync(table);
        }

        public void Update(Table table)
        {
            _context.Tables.Update(table);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
