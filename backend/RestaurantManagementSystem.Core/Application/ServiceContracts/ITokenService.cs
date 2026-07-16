using System.Threading.Tasks;
using RestaurantManagementSystem.Core.Domain.Entities;

namespace RestaurantManagementSystem.Core.Application.ServiceContracts
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}
