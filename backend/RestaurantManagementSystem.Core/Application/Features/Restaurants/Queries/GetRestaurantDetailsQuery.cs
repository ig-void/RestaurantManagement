using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Restaurants.Queries
{
    public record GetRestaurantDetailsQuery(Guid Id) : IRequest<RestaurantDetailsDto?>;

    public class RestaurantDetailsDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public int CuisineTypeId { get; set; }
        public string? CuisineTypeName { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string PhoneNumber { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public decimal AverageCostPerPerson { get; set; }
        public int Capacity { get; set; }
        public int AvailableTableCount { get; set; }
        public required string Status { get; set; }
    }

    public class GetRestaurantDetailsQueryHandler : IRequestHandler<GetRestaurantDetailsQuery, RestaurantDetailsDto?>
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public GetRestaurantDetailsQueryHandler(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<RestaurantDetailsDto?> Handle(GetRestaurantDetailsQuery request, CancellationToken cancellationToken)
        {
            var r = await _restaurantRepository.GetByIdAsync(request.Id);
            if (r == null) return null;

            return new RestaurantDetailsDto
            {
                Id = r.Id,
                Name = r.Name,
                CuisineTypeId = r.CuisineTypeId,
                CuisineTypeName = r.CuisineType?.Name,
                Address = r.Address,
                City = r.City,
                PhoneNumber = r.PhoneNumber,
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                AverageCostPerPerson = r.AverageCostPerPerson,
                Capacity = r.Capacity,
                AvailableTableCount = r.Tables?.Count(t => t.Status == "Available" && !t.IsDeleted) ?? 0,
                Status = r.Status
            };
        }
    }
}
