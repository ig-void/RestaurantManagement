using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public required string FullName { get; set; }
        public required string City { get; set; }
        public string? DietaryPreferences { get; set; } // Comma-separated options: Vegetarian, Vegan, Gluten-Free, No Peanuts
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
