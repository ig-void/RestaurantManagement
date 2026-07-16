using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Reservations.Commands
{
    public record CompleteReservationCommand(
        Guid ReservationId,
        Guid CurrentUserId
    ) : IRequest<ReservationResponse>;

    public class CompleteReservationCommandHandler : IRequestHandler<CompleteReservationCommand, ReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITableRepository _tableRepository;

        public CompleteReservationCommandHandler(
            IReservationRepository reservationRepository,
            ITableRepository tableRepository)
        {
            _reservationRepository = reservationRepository;
            _tableRepository = tableRepository;
        }

        public async Task<ReservationResponse> Handle(CompleteReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId);
            if (reservation == null)
            {
                return new ReservationResponse(false, "Reservation not found.", null);
            }

            if (reservation.Status != "Checked-In")
            {
                return new ReservationResponse(false, $"Reservation must be in 'Checked-In' status to be completed. Current status is '{reservation.Status}'.", null);
            }

            if (reservation.TableId == null)
            {
                return new ReservationResponse(false, "No table is allocated to this reservation.", null);
            }

            var table = await _tableRepository.GetByIdAsync(reservation.TableId.Value);
            if (table == null)
            {
                return new ReservationResponse(false, "Allocated table not found.", null);
            }

            table.Status = "Available";
            table.UpdatedBy = request.CurrentUserId;
            table.UpdatedDate = DateTime.UtcNow;
            _tableRepository.Update(table);

            reservation.Status = "Completed";
            reservation.UpdatedBy = request.CurrentUserId;
            reservation.UpdatedDate = DateTime.UtcNow;
            _reservationRepository.Update(reservation);

            await _reservationRepository.SaveChangesAsync();

            return new ReservationResponse(true, $"Reservation completed successfully. Table {table.TableNumber} is now Available.", reservation.Id);
        }
    }
}
