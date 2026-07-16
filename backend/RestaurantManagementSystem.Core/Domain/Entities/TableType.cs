using System.Collections.Generic;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class TableType
    {
        public int Id { get; set; }
        public required string Name { get; set; } // e.g. "Couple", "Family", "Large Family", "Banquet"
        public int Capacity { get; set; } // e.g. 2, 4, 6, 8

        // Navigation property
        public ICollection<Table> Tables { get; set; } = new List<Table>();
    }
}
