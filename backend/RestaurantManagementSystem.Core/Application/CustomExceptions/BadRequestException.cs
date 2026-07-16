using System;

namespace RestaurantManagementSystem.Core.Application.CustomExceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}
