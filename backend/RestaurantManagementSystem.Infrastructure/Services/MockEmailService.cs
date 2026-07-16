using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantManagementSystem.Core.Application.ServiceContracts;

namespace RestaurantManagementSystem.Infrastructure.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation("================ MOCK EMAIL SENT ================");
            _logger.LogInformation("To: {To}", to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body:\n{Body}", body);
            _logger.LogInformation("=================================================");

            return Task.CompletedTask;
        }
    }
}
