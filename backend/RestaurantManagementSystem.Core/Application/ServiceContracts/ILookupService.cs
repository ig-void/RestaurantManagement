using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Core.Application.ServiceContracts
{
    public record CuisineTypeLookupDto(int Id, string Name);
    public record TableTypeLookupDto(int Id, string Name, int Capacity);
    public record RestaurantLookupDto(Guid Id, string Name, string Status);

    public interface ILookupService
    {
        Task<List<CuisineTypeLookupDto>> GetCuisineTypesAsync();
        Task<List<TableTypeLookupDto>> GetTableTypesAsync();
        Task<List<RestaurantLookupDto>> GetRestaurantLookupAsync();
    }
}
