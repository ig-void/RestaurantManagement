using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Application.ServiceContracts;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Reservations.Commands
{
    public record CancelReservationCommand(
        Guid ReservationId,
        Guid CurrentUserId,
        bool IsManager
    ) : IRequest<ReservationResponse>;

    public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, ReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITableRepository _tableRepository;
        private readonly IEmailService _emailService;

        public CancelReservationCommandHandler(
            IReservationRepository reservationRepository,
            ITableRepository tableRepository,
            IEmailService emailService)
        {
            _reservationRepository = reservationRepository;
            _tableRepository = tableRepository;
            _emailService = emailService;
        }

        public async Task<ReservationResponse> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
            if (reservation == null)
            {
                return new ReservationResponse(false, "Reservation not found.", null);
            }

            if (!request.IsManager && reservation.Status != "Pending")
            {
                return new ReservationResponse(false, "Customers can only cancel reservations that are in 'Pending' status.", null);
            }

            if (reservation.Status == "Completed" || reservation.Status == "Cancelled")
            {
                return new ReservationResponse(false, $"Reservation is already in '{reservation.Status}' status and cannot be cancelled.", null);
            }

            if (reservation.TableId.HasValue)
            {
                var table = await _tableRepository.GetByIdAsync(reservation.TableId.Value);
                if (table != null)
                {
                    table.Status = "Available";
                    table.UpdatedBy = request.CurrentUserId;
                    table.UpdatedDate = DateTime.UtcNow;
                    _tableRepository.Update(table);
                }
            }

            reservation.Status = "Cancelled";
            reservation.UpdatedBy = request.CurrentUserId;
            reservation.UpdatedDate = DateTime.UtcNow;
            _reservationRepository.Update(reservation);

            await _reservationRepository.SaveChangesAsync();

            var customerEmail = reservation.Customer?.Email ?? "customer@example.com";
            var customerName = reservation.Customer?.FullName ?? "Customer";
            var restaurantName = reservation.Restaurant?.Name ?? "our Restaurant";
            var dateStr = reservation.ReservationDate.ToString("yyyy-MM-dd");
            var timeStr = reservation.ReservationTime.ToString("HH:mm");

            var emailBody = $"Dear {customerName},\n\nWe would like to inform you that your reservation at {restaurantName} for {dateStr} at {timeStr} has been CANCELLED.\n\nIf you have any questions, please contact the restaurant manager.\n\nBest regards,\nRestaurant Management Team";

            _ = _emailService.SendEmailAsync(customerEmail, $"Reservation Cancelled - {restaurantName}", emailBody);

            return new ReservationResponse(true, "Reservation cancelled and table released successfully.", reservation.Id);
        }
    }
}
