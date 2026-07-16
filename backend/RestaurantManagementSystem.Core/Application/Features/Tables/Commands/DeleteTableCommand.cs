using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Tables.Commands
{
    public record DeleteTableCommand(
        Guid Id,
        Guid CurrentUserId
    ) : IRequest<TableResponse>;

    public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, TableResponse>
    {
        private readonly ITableRepository _tableRepository;

        public DeleteTableCommandHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<TableResponse> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(request.Id);
            if (table == null)
            {
                return new TableResponse(false, "Table not found.", null);
            }

            if (table.Status == "Occupied")
            {
                return new TableResponse(false, "Cannot delete table because it is currently occupied.", null);
            }

            var activeReservations = table.Reservations?
                .Where(r => r.Status == "Pending" || r.Status == "Confirmed" || r.Status == "Checked-In")
                .ToList();

            if (activeReservations != null && activeReservations.Any())
            {
                return new TableResponse(false, "Cannot delete table because it has active reservations.", null);
            }

            table.IsDeleted = true;
            table.DeletedBy = request.CurrentUserId;
            table.DeletedDate = DateTime.UtcNow;

            _tableRepository.Update(table);
            await _tableRepository.SaveChangesAsync();

            return new TableResponse(true, "Table deleted successfully.", table.Id);
        }
    }
}
