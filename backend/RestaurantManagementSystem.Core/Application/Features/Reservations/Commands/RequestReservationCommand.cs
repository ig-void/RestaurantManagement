using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Entities;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Reservations.Commands
{
    public record RequestReservationCommand(
        Guid RestaurantId,
        int GuestCount,
        DateOnly ReservationDate,
        TimeOnly ReservationTime,
        string? SpecialRequests,
        Guid CustomerId
    ) : IRequest<ReservationResponse>;

    public record ReservationResponse(
        bool Success,
        string Message,
        Guid? ReservationId
    );

    public class RequestReservationCommandValidator : AbstractValidator<RequestReservationCommand>
    {
        public RequestReservationCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("Restaurant is required.");
            
            RuleFor(x => x.GuestCount)
                .InclusiveBetween(1, 10).WithMessage("Guest Count must be between 1 and 10.");

            RuleFor(x => x.ReservationDate)
                .NotEmpty().WithMessage("Reservation Date is required.")
                .Must(date => date >= DateOnly.FromDateTime(DateTime.Today) && date <= DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
                .WithMessage("Reservation Date must be today or within the next 30 days.");

            RuleFor(x => x.ReservationTime).NotEmpty().WithMessage("Reservation Time is required.");
        }
    }

    public class RequestReservationCommandHandler : IRequestHandler<RequestReservationCommand, ReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRestaurantRepository _restaurantRepository;

        public RequestReservationCommandHandler(
            IReservationRepository reservationRepository,
            IRestaurantRepository restaurantRepository)
        {
            _reservationRepository = reservationRepository;
            _restaurantRepository = restaurantRepository;
        }

        public async Task<ReservationResponse> Handle(RequestReservationCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
            if (restaurant == null)
            {
                return new ReservationResponse(false, "Restaurant not found.", null);
            }

            if (request.GuestCount > restaurant.Capacity)
            {
                return new ReservationResponse(false, $"Guest count exceeds the restaurant capacity of {restaurant.Capacity}.", null);
            }

            if (request.ReservationTime < restaurant.OpeningTime || request.ReservationTime > restaurant.ClosingTime)
            {
                return new ReservationResponse(false, $"Reservation time must be between the operating hours of {restaurant.OpeningTime} and {restaurant.ClosingTime}.", null);
            }

            var activeCount = await _reservationRepository.GetActiveReservationsCountForCustomerAsync(request.CustomerId);
            if (activeCount >= 5)
            {
                return new ReservationResponse(false, "You cannot have more than 5 active reservations at a time.", null);
            }

            var isDuplicate = await _reservationRepository.HasDuplicateActiveReservationAsync(
                request.CustomerId,
                request.RestaurantId,
                request.ReservationDate
            );
            if (isDuplicate)
            {
                return new ReservationResponse(false, "You already have an active reservation at this restaurant on this date.", null);
            }

            var hasSuitableTable = await _reservationRepository.HasSuitableTableAsync(request.RestaurantId, request.GuestCount);
            if (!hasSuitableTable)
            {
                return new ReservationResponse(false, "No suitable table size is available at this restaurant for the requested guest count.", null);
            }

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                RestaurantId = request.RestaurantId,
                TableId = null, // Not allocated yet
                GuestCount = request.GuestCount,
                ReservationDate = request.ReservationDate,
                ReservationTime = request.ReservationTime,
                SpecialRequests = request.SpecialRequests,
                Status = "Pending",
                RequestedOn = DateTime.UtcNow
            };

            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveChangesAsync();

            return new ReservationResponse(true, "Reservation request submitted successfully and is pending manager confirmation.", reservation.Id);
        }
    }
}
