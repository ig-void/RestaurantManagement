using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Restaurants.Commands
{
    public record UpdateRestaurantCommand(
        Guid Id,
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

    public class UpdateRestaurantCommandValidator : AbstractValidator<UpdateRestaurantCommand>
    {
        public UpdateRestaurantCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Restaurant ID is required.");
            
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

    public class UpdateRestaurantCommandHandler : IRequestHandler<UpdateRestaurantCommand, RestaurantResponse>
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IReservationRepository _reservationRepository;

        public UpdateRestaurantCommandHandler(
            IRestaurantRepository restaurantRepository,
            IReservationRepository reservationRepository)
        {
            _restaurantRepository = restaurantRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<RestaurantResponse> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(request.Id);
            if (restaurant == null)
            {
                return new RestaurantResponse(false, "Restaurant not found.", null);
            }

            var existingWithName = await _restaurantRepository.GetByNameAsync(request.Name);
            if (existingWithName != null && existingWithName.Id != request.Id)
            {
                return new RestaurantResponse(false, "Another restaurant with this name already exists.", null);
            }

            var activeReservationsList = await _reservationRepository.GetPagedAsync(
                restaurantId: request.Id,
                status: null, 
                date: null,
                searchTerm: null,
                sortBy: null,
                isAscending: true,
                pageNumber: 1,
                pageSize: 9999
            );

            var activeReservations = activeReservationsList.Items
                .Where(r => r.Status == "Pending" || r.Status == "Confirmed" || r.Status == "Checked-In")
                .ToList();

            var activeGuestsCount = activeReservations.Sum(r => r.GuestCount);
            if (request.Capacity < activeGuestsCount)
            {
                return new RestaurantResponse(false, $"Cannot reduce capacity to {request.Capacity}. The restaurant currently has {activeGuestsCount} active reserved/checked-in guests.", null);
            }

            if (request.Status == "Inactive")
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var activeToday = activeReservations.Any(r => r.ReservationDate == today);
                if (activeToday)
                {
                    return new RestaurantResponse(false, "Cannot set restaurant status to Inactive because there are active reservations scheduled for today.", null);
                }
            }

            restaurant.Name = request.Name;
            restaurant.CuisineTypeId = request.CuisineTypeId;
            restaurant.Address = request.Address;
            restaurant.City = request.City;
            restaurant.PhoneNumber = request.PhoneNumber;
            restaurant.OpeningTime = request.OpeningTime;
            restaurant.ClosingTime = request.ClosingTime;
            restaurant.AverageCostPerPerson = request.AverageCostPerPerson;
            restaurant.Capacity = request.Capacity;
            restaurant.Status = request.Status;
            restaurant.UpdatedBy = request.CurrentUserId;
            restaurant.UpdatedDate = DateTime.UtcNow;

            _restaurantRepository.Update(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            return new RestaurantResponse(true, "Restaurant updated successfully.", restaurant.Id);
        }
    }
}
