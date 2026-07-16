using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Common;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Reservations.Queries
{
    public record GetReservationListQuery(
        Guid? RestaurantId,
        string? Status,
        DateOnly? ReservationDate,
        string? SearchTerm,
        string? SortBy,
        bool IsAscending = false, // Sort descending by default to show newest
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedList<ReservationListDto>>;

    public class ReservationListDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public Guid RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public Guid? TableId { get; set; }
        public string? TableNumber { get; set; }
        public string? TableTypeName { get; set; }
        public int GuestCount { get; set; }
        public DateOnly ReservationDate { get; set; }
        public TimeOnly ReservationTime { get; set; }
        public string? SpecialRequests { get; set; }
        public string? Status { get; set; }
        public DateTime RequestedOn { get; set; }
    }

    public class GetReservationListQueryHandler : IRequestHandler<GetReservationListQuery, PaginatedList<ReservationListDto>>
    {
        private readonly IReservationRepository _reservationRepository;

        public GetReservationListQueryHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<PaginatedList<ReservationListDto>> Handle(GetReservationListQuery request, CancellationToken cancellationToken)
        {
            var paged = await _reservationRepository.GetPagedAsync(
                restaurantId: request.RestaurantId,
                status: request.Status,
                date: request.ReservationDate,
                searchTerm: request.SearchTerm,
                sortBy: request.SortBy,
                isAscending: request.IsAscending,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize
            );

            var dtos = paged.Items.Select(r => new ReservationListDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer?.FullName,
                CustomerEmail = r.Customer?.Email,
                CustomerPhone = r.Customer?.PhoneNumber,
                RestaurantId = r.RestaurantId,
                RestaurantName = r.Restaurant?.Name,
                TableId = r.TableId,
                TableNumber = r.Table?.TableNumber,
                TableTypeName = r.Table?.TableType?.Name,
                GuestCount = r.GuestCount,
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime,
                SpecialRequests = r.SpecialRequests,
                Status = r.Status,
                RequestedOn = r.RequestedOn
            }).ToList();

            return new PaginatedList<ReservationListDto>(
                dtos,
                paged.TotalCount,
                paged.PageNumber,
                request.PageSize
            );
        }
    }
}
