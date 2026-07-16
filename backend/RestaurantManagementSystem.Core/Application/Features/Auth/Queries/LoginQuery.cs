using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RestaurantManagementSystem.Core.Application.ServiceContracts;
using RestaurantManagementSystem.Core.Domain.Entities;

namespace RestaurantManagementSystem.Core.Application.Features.Auth.Queries
{
    public record LoginQuery(
        string Email,
        string Password
    ) : IRequest<LoginResponse>;

    public record LoginResponse(
        bool Success,
        string Message,
        string? Token,
        string? FullName,
        string? Email,
        string? Role
    );

    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }

    public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public LoginQueryHandler(
            UserManager<User> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginResponse(false, "Invalid email or password.", null, null, null, null);
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValidPassword)
            {
                return new LoginResponse(false, "Invalid email or password.", null, null, null, null);
            }

            var token = await _tokenService.GenerateTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Customer";

            return new LoginResponse(
                Success: true,
                Message: "Login successful.",
                Token: token,
                FullName: user.FullName,
                Email: user.Email,
                Role: primaryRole
            );
        }
    }
}
