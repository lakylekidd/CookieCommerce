using System.Net;
using System.Text.Json;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.CustomerDomain.Api.Middleware;

/// <summary>
/// Middleware that catches domain exceptions and maps them to appropriate HTTP status codes.
/// CustomerProfileValidationException with field errors → 422 ValidationProblemDetails.
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An exception occurred: {ExceptionType}", exception.GetType().Name);

        context.Response.ContentType = "application/problem+json";

        if (exception is CustomerAddressValidationException addressValidation)
        {
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            var vpd = new ValidationProblemDetails(addressValidation.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
            {
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Title = "Address Validation Failed",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(vpd,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return;
        }

        // CustomerProfileValidationException with field-level errors → 422 ValidationProblemDetails
        if (exception is CustomerProfileValidationException profileValidation
            && profileValidation.FieldErrors.Count > 0)
        {
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            var vpd = new ValidationProblemDetails(
                profileValidation.FieldErrors.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value))
            {
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Title = "Profile Validation Failed",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(vpd,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return;
        }

        var (statusCode, problemDetails) = exception switch
        {
            CustomerNotFoundException notFound => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Customer Not Found",
                    Detail = notFound.Message,
                    Instance = context.Request.Path
                }
            ),
            MissingCustomerProfileException missingProfile => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Profile Not Found",
                    Detail = "Customer profile does not exist or has been forgotten.",
                    Instance = context.Request.Path
                }
            ),
            DuplicateCustomerEmailException => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Email Address Conflict",
                    Detail = "Email address is already associated with another account.",
                    Instance = context.Request.Path
                }
            ),
            CustomerProfileValidationException validation => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Title = "Profile Validation Failed",
                    Detail = validation.Message,
                    Instance = context.Request.Path
                }
            ),
            UnauthorizedCustomerProfileAccessException => (
                HttpStatusCode.Forbidden,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = "Forbidden",
                    Detail = "You are not authorised to access this profile.",
                    Instance = context.Request.Path
                }
            ),
            InvalidEmailVerificationTokenException invalidToken => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Title = "Invalid Email Verification Token",
                    Detail = invalidToken.Message,
                    Instance = context.Request.Path
                }
            ),
            ExpiredEmailVerificationTokenException expiredToken => (
                HttpStatusCode.Gone,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Gone,
                    Title = "Expired Email Verification Token",
                    Detail = expiredToken.Message,
                    Instance = context.Request.Path
                }
            ),
            CustomerAddressNotFoundException => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Address Not Found",
                    Detail = "Address was not found for this customer.",
                    Instance = context.Request.Path
                }
            ),
            AddressNotFoundException addressNotFound => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Address Not Found",
                    Detail = addressNotFound.Message,
                    Instance = context.Request.Path
                }
            ),
            DuplicateAddressLabelException duplicate => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Duplicate Address Label",
                    Detail = "An active address with this label already exists.",
                    Instance = context.Request.Path
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "An error occurred while processing your request",
                    Detail = exception.Message,
                    Instance = context.Request.Path
                }
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method to register the domain exception middleware
/// </summary>
public static class DomainExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseDomainExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DomainExceptionMiddleware>();
    }
}
