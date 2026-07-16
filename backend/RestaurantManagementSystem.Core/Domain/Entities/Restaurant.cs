using System;
using System.Collections.Generic;
using RestaurantManagementSystem.Core.Domain.Common;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class Restaurant : IAuditableEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public int CuisineTypeId { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string PhoneNumber { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public decimal AverageCostPerPerson { get; set; }
        public int Capacity { get; set; }
        public required string Status { get; set; } // "Active", "Inactive", "Maintenance"

        // Audit Fields
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        // Navigation properties
        public CuisineType? CuisineType { get; set; }
        public ICollection<Table> Tables { get; set; } = new List<Table>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
