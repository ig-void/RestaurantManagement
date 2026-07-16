using System.Threading.Tasks;

namespace RestaurantManagementSystem.Core.Application.ServiceContracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
