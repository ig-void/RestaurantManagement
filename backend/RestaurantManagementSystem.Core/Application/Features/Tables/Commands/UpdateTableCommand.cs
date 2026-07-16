using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Tables.Commands
{
    public record UpdateTableCommand(
        Guid Id,
        string TableNumber,
        int TableTypeId,
        int SeatingCapacity,
        string Status,
        Guid CurrentUserId
    ) : IRequest<TableResponse>;

    public class UpdateTableCommandValidator : AbstractValidator<UpdateTableCommand>
    {
        public UpdateTableCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Table ID is required.");
            RuleFor(x => x.TableNumber).NotEmpty().WithMessage("Table Number is required.").MaximumLength(50);
            RuleFor(x => x.TableTypeId).GreaterThan(0).WithMessage("Table Type is required.");
            RuleFor(x => x.SeatingCapacity).GreaterThan(0).WithMessage("Seating Capacity must be greater than 0.");
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status == "Available" || status == "Reserved" || status == "Occupied" || status == "Maintenance")
                .WithMessage("Status must be Available, Reserved, Occupied, or Maintenance.");
        }
    }

    public class UpdateTableCommandHandler : IRequestHandler<UpdateTableCommand, TableResponse>
    {
        private readonly ITableRepository _tableRepository;

        public UpdateTableCommandHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<TableResponse> Handle(UpdateTableCommand request, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(request.Id);
            if (table == null)
            {
                return new TableResponse(false, "Table not found.", null);
            }

            var existingWithNumber = await _tableRepository.GetByNumberAndRestaurantAsync(table.RestaurantId, request.TableNumber);
            if (existingWithNumber != null && existingWithNumber.Id != request.Id && !existingWithNumber.IsDeleted)
            {
                return new TableResponse(false, $"Table number '{request.TableNumber}' already exists in this restaurant.", null);
            }

            var isValidCapacity = AddTableCommandHandler.ValidateTableTypeCapacity(request.TableTypeId, request.SeatingCapacity);
            if (!isValidCapacity)
            {
                return new TableResponse(false, $"Seating capacity {request.SeatingCapacity} does not match the selected Table Type.", null);
            }

            if (table.Status == "Occupied" && request.Status == "Maintenance")
            {
                return new TableResponse(false, "Cannot put an occupied table under maintenance. Complete or cancel active reservations first.", null);
            }

            if (table.Status == "Reserved" && request.Status == "Available")
            {
                return new TableResponse(false, "Reserved tables cannot be manually marked Available. Complete or cancel the active reservation instead.", null);
            }

            var activeReservations = table.Reservations?
                .Where(r => r.Status == "Pending" || r.Status == "Confirmed" || r.Status == "Checked-In")
                .ToList();

            if (activeReservations != null && activeReservations.Any())
            {
                var maxGuestCount = activeReservations.Max(r => r.GuestCount);
                if (request.SeatingCapacity < maxGuestCount)
                {
                    return new TableResponse(false, $"Cannot reduce seating capacity to {request.SeatingCapacity}. An active reservation requires a capacity of at least {maxGuestCount}.", null);
                }
            }

            table.TableNumber = request.TableNumber;
            table.TableTypeId = request.TableTypeId;
            table.SeatingCapacity = request.SeatingCapacity;
            table.Status = request.Status;
            table.UpdatedBy = request.CurrentUserId;
            table.UpdatedDate = DateTime.UtcNow;

            _tableRepository.Update(table);
            await _tableRepository.SaveChangesAsync();

            return new TableResponse(true, "Table updated successfully.", table.Id);
        }
    }
}
