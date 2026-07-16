using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Application.ServiceContracts;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Reservations.Commands
{
    public record ConfirmReservationCommand(
        Guid ReservationId,
        Guid CurrentUserId
    ) : IRequest<ReservationResponse>;

    public class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, ReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IEmailService _emailService;

        public ConfirmReservationCommandHandler(
            IReservationRepository reservationRepository,
            ITableRepository tableRepository,
            IEmailService emailService)
        {
            _reservationRepository = reservationRepository;
            _tableRepository = tableRepository;
            _emailService = emailService;
        }

        public async Task<ReservationResponse> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
            if (reservation == null)
            {
                return new ReservationResponse(false, "Reservation not found.", null);
            }

            if (reservation.Status != "Pending")
            {
                return new ReservationResponse(false, $"Reservation is already in '{reservation.Status}' status and cannot be confirmed.", null);
            }

            var bestTable = await _reservationRepository.GetBestMatchingAvailableTableAsync(
                reservation.RestaurantId,
                reservation.GuestCount
            );

            if (bestTable == null)
            {
                return new ReservationResponse(false, "No available table matches the required seating capacity at this time.", null);
            }

            bestTable.Status = "Reserved";
            bestTable.UpdatedBy = request.CurrentUserId;
            bestTable.UpdatedDate = DateTime.UtcNow;
            _tableRepository.Update(bestTable);

            reservation.TableId = bestTable.Id;
            reservation.Status = "Confirmed";
            reservation.UpdatedBy = request.CurrentUserId;
            reservation.UpdatedDate = DateTime.UtcNow;
            _reservationRepository.Update(reservation);

            await _reservationRepository.SaveChangesAsync();

            var customerEmail = reservation.Customer?.Email ?? "customer@example.com";
            var customerName = reservation.Customer?.FullName ?? "Customer";
            var restaurantName = reservation.Restaurant?.Name ?? "our Restaurant";
            var dateStr = reservation.ReservationDate.ToString("yyyy-MM-dd");
            var timeStr = reservation.ReservationTime.ToString("HH:mm");
            var tableNum = bestTable.TableNumber;

            var emailBody = $"Dear {customerName},\n\nYour reservation at {restaurantName} has been CONFIRMED!\n\nDetails:\n- Date: {dateStr}\n- Time: {timeStr}\n- Guests: {reservation.GuestCount}\n- Table Number: {tableNum}\n\nWe look forward to serving you!\n\nBest regards,\nRestaurant Management Team";
            
            _ = _emailService.SendEmailAsync(customerEmail, $"Reservation Confirmed - {restaurantName}", emailBody);

            return new ReservationResponse(true, $"Reservation confirmed and allocated to Table {bestTable.TableNumber} successfully.", reservation.Id);
        }
    }
}
