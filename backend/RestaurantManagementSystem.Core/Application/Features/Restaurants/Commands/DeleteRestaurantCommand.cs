using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantManagementSystem.Core.Domain.RepositoryContracts;

namespace RestaurantManagementSystem.Core.Application.Features.Restaurants.Commands
{
    public record DeleteRestaurantCommand(
        Guid Id,
        Guid CurrentUserId
    ) : IRequest<RestaurantResponse>;

    public class DeleteRestaurantCommandHandler : IRequestHandler<DeleteRestaurantCommand, RestaurantResponse>
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IReservationRepository _reservationRepository;

        public DeleteRestaurantCommandHandler(
            IRestaurantRepository restaurantRepository,
            IReservationRepository reservationRepository)
        {
            _restaurantRepository = restaurantRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<RestaurantResponse> Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(request.Id);
            if (restaurant == null)
            {
                return new RestaurantResponse(false, "Restaurant not found.", null);
            }

            if (restaurant.Tables != null && restaurant.Tables.Any(t => !t.IsDeleted))
            {
                return new RestaurantResponse(false, "Cannot delete restaurant because it contains active tables. Delete all tables first.", null);
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

            var hasActiveReservations = activeReservationsList.Items
                .Any(r => r.Status == "Pending" || r.Status == "Confirmed" || r.Status == "Checked-In");

            if (hasActiveReservations)
            {
                return new RestaurantResponse(false, "Cannot delete restaurant because it has active reservations.", null);
            }

            restaurant.IsDeleted = true;
            restaurant.DeletedBy = request.CurrentUserId;
            restaurant.DeletedDate = DateTime.UtcNow;

            _restaurantRepository.Update(restaurant);
            await _restaurantRepository.SaveChangesAsync();

            return new RestaurantResponse(true, "Restaurant deleted successfully.", restaurant.Id);
        }
    }
}
