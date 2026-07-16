using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Restaurants.Queries
{
    public record GetRestaurantListQuery(
        int? CuisineTypeId,
        string? Status,
        string? SearchTerm,
        string? SortBy,
        bool IsAscending = true,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedList<RestaurantListDto>>;

    public class RestaurantListDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string CuisineTypeName { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string PhoneNumber { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public decimal AverageCostPerPerson { get; set; }
        public int Capacity { get; set; }
        public int AvailableTableCount { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class GetRestaurantListQueryHandler : IRequestHandler<GetRestaurantListQuery, PaginatedList<RestaurantListDto>>
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public GetRestaurantListQueryHandler(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<PaginatedList<RestaurantListDto>> Handle(GetRestaurantListQuery request, CancellationToken cancellationToken)
        {
            var pagedRestaurants = await _restaurantRepository.GetPagedAsync(
                cuisineTypeId: request.CuisineTypeId,
                status: request.Status,
                searchTerm: request.SearchTerm,
                sortBy: request.SortBy,
                isAscending: request.IsAscending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize
            );

            var dtos = pagedRestaurants.Items.Select(r => new RestaurantListDto
            {
                Id = r.Id,
                Name = r.Name,
                CuisineTypeName = r.CuisineType?.Name ?? "Unknown",
                Address = r.Address,
                City = r.City,
                PhoneNumber = r.PhoneNumber,
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                AverageCostPerPerson = r.AverageCostPerPerson,
                Capacity = r.Capacity,
                AvailableTableCount = r.Tables?.Count(t => t.Status == "Available" && !t.IsDeleted) ?? 0,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                UpdatedDate = r.UpdatedDate
            }).ToList();

            return new PaginatedList<RestaurantListDto>(
                dtos,
                pagedRestaurants.TotalCount,
                pagedRestaurants.PageNumber,
                request.PageSize
            );
        }
    }
}
