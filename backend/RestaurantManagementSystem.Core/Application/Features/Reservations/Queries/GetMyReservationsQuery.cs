using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Reservations.Queries
{
    public record GetMyReservationsQuery(
        Guid CustomerId,
        string? SearchTerm,
        string? SortBy,
        bool IsAscending = false, // Sort descending by default (newest first)
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedList<CustomerReservationDto>>;

    public class CustomerReservationDto
    {
        public Guid Id { get; set; }
        public required string RestaurantName { get; set; }
        public required string CuisineTypeName { get; set; }
        public DateOnly ReservationDate { get; set; }
        public TimeOnly ReservationTime { get; set; }
        public int GuestCount { get; set; }
        public string? TableNumber { get; set; }
        public string? TableTypeName { get; set; }
        public required string Status { get; set; }
        public string? SpecialRequests { get; set; }
    }

    public class GetMyReservationsQueryHandler : IRequestHandler<GetMyReservationsQuery, PaginatedList<CustomerReservationDto>>
    {
        private readonly IReservationRepository _reservationRepository;

        public GetMyReservationsQueryHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<PaginatedList<CustomerReservationDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
        {
            var paged = await _reservationRepository.GetCustomerPagedAsync(
                customerId: request.CustomerId,
                searchTerm: request.SearchTerm,
                sortBy: request.SortBy,
                isAscending: request.IsAscending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize
            );

            var dtos = paged.Items.Select(r => new CustomerReservationDto
            {
                Id = r.Id,
                RestaurantName = r.Restaurant?.Name ?? "Unknown",
                CuisineTypeName = r.Restaurant?.CuisineType?.Name ?? "Unknown",
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime,
                GuestCount = r.GuestCount,
                TableNumber = r.Table?.TableNumber,
                TableTypeName = r.Table?.TableType?.Name,
                Status = r.Status,
                SpecialRequests = r.SpecialRequests
            }).ToList();

            return new PaginatedList<CustomerReservationDto>(
                dtos,
                paged.TotalCount,
                paged.PageNumber,
                request.PageSize
            );
        }
    }
}
