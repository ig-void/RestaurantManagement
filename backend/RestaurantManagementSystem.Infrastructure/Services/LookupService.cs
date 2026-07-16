using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Core.Application.ServiceContracts;
using RestaurantManagementSystem.Infrastructure.Data;

namespace RestaurantManagementSystem.Infrastructure.Services
{
    public class LookupService : ILookupService
    {
        private readonly AppDbContext _context;

        public LookupService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CuisineTypeLookupDto>> GetCuisineTypesAsync()
        {
            return await _context.CuisineTypes
                .Select(c => new CuisineTypeLookupDto(c.Id, c.Name))
                .ToListAsync();
        }

        public async Task<List<TableTypeLookupDto>> GetTableTypesAsync()
        {
            return await _context.TableTypes
                .Select(t => new TableTypeLookupDto(t.Id, t.Name, t.Capacity))
                .ToListAsync();
        }

        public async Task<List<RestaurantLookupDto>> GetRestaurantLookupAsync()
        {
            return await _context.Restaurants
                .Select(r => new RestaurantLookupDto(r.Id, r.Name, r.Status))
                .ToListAsync();
        }
    }
}
