using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantManagementSystem.Core.Domain.Entities;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Restaurants.Commands
{
    public record AddRestaurantCommand(
        string Name,
        int CuisineTypeId,
        string Address,
        string City,
        string PhoneNumber,
        TimeOnly OpeningTime,
        TimeOnly ClosingTime,
        decimal AverageCostPerPerson,
        int Capacity,
        string Status,
        Guid CurrentUserId
    ) : IRequest<RestaurantResponse>;

    public record RestaurantResponse(
        bool Success,
        string Message,
        Guid? RestaurantId
    );

    public class AddRestaurantCommandValidator : AbstractValidator<AddRestaurantCommand>
    {
        public AddRestaurantCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Restaurant Name is required.")
                .MaximumLength(100).WithMessage("Restaurant Name cannot exceed 100 characters.");

            RuleFor(x => x.CuisineTypeId)
                .GreaterThan(0).WithMessage("Valid Cuisine Type is required.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone Number is required.")
                .MaximumLength(20).WithMessage("Phone Number cannot exceed 20 characters.");

            RuleFor(x => x.ClosingTime)
                .GreaterThan(x => x.OpeningTime).WithMessage("Closing Time must be greater than Opening Time.");

            RuleFor(x => x.AverageCostPerPerson)
                .GreaterThan(0).WithMessage("Average Cost must be greater than 0.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status == "Active" || status == "Inactive" || status == "Maintenance")
                .WithMessage("Status must be Active, Inactive, or Maintenance.");
        }
    }

    public class AddRestaurantCommandHandler : IRequestHandler<AddRestaurantCommand, RestaurantResponse>
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public AddRestaurantCommandHandler(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<RestaurantResponse> Handle(AddRestaurantCommand request, CancellationToken cancellationToken)
        {
            var existing = await _restaurantRepository.GetByNameAsync(request.Name);
            if (existing != null)
            {
                return new RestaurantResponse(false, "Restaurant Name must be unique.", null);
            }

            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CuisineTypeId = request.CuisineTypeId,
                Address = request.Address,
                City = request.City,
                PhoneNumber = request.PhoneNumber,
                OpeningTime = request.OpeningTime,
                ClosingTime = request.ClosingTime,
                AverageCostPerPerson = request.AverageCostPerPerson,
                Capacity = request.Capacity,
                Status = request.Status,
                CreatedBy = request.CurrentUserId,
                CreatedDate = DateTime.UtcNow
            };

            await _restaurantRepository.AddAsync(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            return new RestaurantResponse(true, "Restaurant added successfully.", restaurant.Id);
        }
    }
}
