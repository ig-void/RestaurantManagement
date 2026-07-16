using System;
using Microsoft.AspNetCore.Identity;

namespace RestaurantManagementSystem.Core.Domain.Entities
{
    public class Role : IdentityRole<Guid>
    {
        // Name and Id are inherited from IdentityRole<Guid>
    }
}
