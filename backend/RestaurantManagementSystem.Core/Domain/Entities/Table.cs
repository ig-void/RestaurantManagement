using System;
using System.Collections.Generic;
using RestaurantManagementSystem.Core.Domain.Common;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class Table : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public required string TableNumber { get; set; }
        public int TableTypeId { get; set; }
        public int SeatingCapacity { get; set; }
        public required string Status { get; set; } // "Available", "Reserved", "Occupied", "Maintenance"

        // Audit Fields
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        // Navigation properties
        public Restaurant? Restaurant { get; set; }
        public TableType? TableType { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
