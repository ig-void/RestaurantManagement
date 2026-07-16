using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestaurantManagementSystem.Core.Application.CustomExceptions;

namespace RestaurantManagementSystem.WebApi.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var response = new ExceptionResponse();

            switch (exception)
            {
                case ValidationException valEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = "One or more validation failures have occurred.";
                    response.Errors = valEx.Errors;
                    break;

                case NotFoundException nfEx:
                    statusCode = HttpStatusCode.NotFound;
                    response.Message = nfEx.Message;
                    break;

                case BadRequestException brEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Message = brEx.Message;
                    break;

                default:
                    response.Message = "An unexpected error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(result);
        }
    }

    public class ExceptionResponse
    {
        public bool Success { get; } = false;
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
    }
}
