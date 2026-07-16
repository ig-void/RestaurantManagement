using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Application.ServiceContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Lookups.Queries
{
    // Cuisine Types Lookup
    public record GetCuisineTypesQuery : IRequest<List<CuisineTypeLookupDto>>;

    public class GetCuisineTypesQueryHandler : IRequestHandler<GetCuisineTypesQuery, List<CuisineTypeLookupDto>>
    {
        private readonly ILookupService _lookupService;

        public GetCuisineTypesQueryHandler(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        public async Task<List<CuisineTypeLookupDto>> Handle(GetCuisineTypesQuery request, CancellationToken cancellationToken)
        {
            return await _lookupService.GetCuisineTypesAsync();
        }
    }

    // Table Types Lookup
    public record GetTableTypesQuery : IRequest<List<TableTypeLookupDto>>;

    public class GetTableTypesQueryHandler : IRequestHandler<GetTableTypesQuery, List<TableTypeLookupDto>>
    {
        private readonly ILookupService _lookupService;

        public GetTableTypesQueryHandler(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        public async Task<List<TableTypeLookupDto>> Handle(GetTableTypesQuery request, CancellationToken cancellationToken)
        {
            return await _lookupService.GetTableTypesAsync();
        }
    }

    // Restaurant Lookup
    public record GetRestaurantLookupQuery : IRequest<List<RestaurantLookupDto>>;

    public class GetRestaurantLookupQueryHandler : IRequestHandler<GetRestaurantLookupQuery, List<RestaurantLookupDto>>
    {
        private readonly ILookupService _lookupService;

        public GetRestaurantLookupQueryHandler(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        public async Task<List<RestaurantLookupDto>> Handle(GetRestaurantLookupQuery request, CancellationToken cancellationToken)
        {
            return await _lookupService.GetRestaurantLookupAsync();
        }
    }
}
