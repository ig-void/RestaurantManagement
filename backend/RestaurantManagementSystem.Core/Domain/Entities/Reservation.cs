using System;
using RestaurantManagementSystem.Core.Domain.Common;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class Reservation : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid? TableId { get; set; } // Nullable, allocated upon Manager confirmation
        public int GuestCount { get; set; }
        public DateOnly ReservationDate { get; set; }
        public TimeOnly ReservationTime { get; set; }
        public string? SpecialRequests { get; set; }
        public required string Status { get; set; } // "Pending", "Confirmed", "Checked-In", "Completed", "Cancelled"
        public DateTime RequestedOn { get; set; } = DateTime.UtcNow;

        // IAuditableEntity mapping
        public Guid CreatedBy { get => CustomerId; set => CustomerId = value; }
        public DateTime CreatedDate { get => RequestedOn; set => RequestedOn = value; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        // Navigation properties
        public User? Customer { get; set; }
        public Restaurant? Restaurant { get; set; }
        public Table? Table { get; set; }
    }
}
