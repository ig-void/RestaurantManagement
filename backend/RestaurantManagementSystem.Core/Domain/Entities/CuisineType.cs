using System.Collections.Generic;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class CuisineType
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // Navigation property
        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
    }
}
