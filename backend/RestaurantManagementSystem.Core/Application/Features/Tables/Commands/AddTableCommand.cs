using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Entities;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Tables.Commands
{
    public record AddTableCommand(
        Guid RestaurantId,
        string TableNumber,
        int TableTypeId,
        int SeatingCapacity,
        string Status,
        Guid CurrentUserId
    ) : IRequest<TableResponse>;

    public record TableResponse(
        bool Success,
        string Message,
        Guid? TableId
    );

    public class AddTableCommandValidator : AbstractValidator<AddTableCommand>
    {
        public AddTableCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("Restaurant is required.");
            RuleFor(x => x.TableNumber).NotEmpty().WithMessage("Table Number is required.").MaximumLength(50);
            RuleFor(x => x.TableTypeId).GreaterThan(0).WithMessage("Table Type is required.");
            RuleFor(x => x.SeatingCapacity).GreaterThan(0).WithMessage("Seating Capacity must be greater than 0.");
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status == "Available" || status == "Reserved" || status == "Occupied" || status == "Maintenance")
                .WithMessage("Status must be Available, Reserved, Occupied, or Maintenance.");
        }
    }

    public class AddTableCommandHandler : IRequestHandler<AddTableCommand, TableResponse>
    {
        private readonly ITableRepository _tableRepository;

        public AddTableCommandHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<TableResponse> Handle(AddTableCommand request, CancellationToken cancellationToken)
        {
            var isValidCapacity = ValidateTableTypeCapacity(request.TableTypeId, request.SeatingCapacity);
            if (!isValidCapacity)
            {
                return new TableResponse(false, $"Seating capacity {request.SeatingCapacity} does not match the selected Table Type.", null);
            }

            var existing = await _tableRepository.GetByNumberAndRestaurantAsync(request.RestaurantId, request.TableNumber);
            if (existing != null && !existing.IsDeleted)
            {
                return new TableResponse(false, $"Table number '{request.TableNumber}' already exists in this restaurant.", null);
            }

            var table = new Table
            {
                Id = Guid.NewGuid(),
                RestaurantId = request.RestaurantId,
                TableNumber = request.TableNumber,
                TableTypeId = request.TableTypeId,
                SeatingCapacity = request.SeatingCapacity,
                Status = request.Status,
                CreatedBy = request.CurrentUserId,
                CreatedDate = DateTime.UtcNow
            };

            await _tableRepository.AddAsync(table);
            await _tableRepository.SaveChangesAsync();

            return new TableResponse(true, "Table added successfully.", table.Id);
        }

        public static bool ValidateTableTypeCapacity(int tableTypeId, int seatingCapacity)
        {
            return tableTypeId switch
            {
                1 => seatingCapacity == 2, // Seating 2
                2 => seatingCapacity == 4, // Seating 4
                3 => seatingCapacity == 6, // Seating 6
                4 => seatingCapacity >= 8, // Seating 8+
                _ => false
            };
        }
    }
}
