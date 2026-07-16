using System.Net;
using System.Text.Json;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CookieCommerce.ProductDomain.Api.Middleware;

/// <summary>
/// Middleware that catches domain exceptions and maps them to appropriate HTTP status codes
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
        var (statusCode, problemDetails) = exception switch
        {
            ProductNotFoundException notFound => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Product Not Found",
                    Detail = notFound.Message,
                    Instance = context.Request.Path
                }
            ),
            InvalidPriceException invalidPrice => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Price",
                    Detail = invalidPrice.Message,
                    Instance = context.Request.Path
                }
            ),
            CannotRenameDeactivatedProductException cannotRename => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Cannot Rename Deactivated Product",
                    Detail = cannotRename.Message,
                    Instance = context.Request.Path
                }
            ),
            CannotChangePriceOfDeactivatedProductException cannotChangePrice => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Cannot Change Price of Deactivated Product",
                    Detail = cannotChangePrice.Message,
                    Instance = context.Request.Path
                }
            ),
            DuplicateProductVariantException duplicateVariant => (
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["variants"] = [duplicateVariant.Message]
                })
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Duplicate Product Variant",
                    Detail = duplicateVariant.Message,
                    Instance = context.Request.Path
                }
            ),
            InvalidProductVariantException invalidVariant => (
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["variants"] = [invalidVariant.Message]
                })
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Product Variant",
                    Detail = invalidVariant.Message,
                    Instance = context.Request.Path
                }
            ),
            InvalidProductMediaOwnershipException invalidMedia => (
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["media"] = [invalidMedia.Message]
                })
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Product Media Ownership",
                    Detail = invalidMedia.Message,
                    Instance = context.Request.Path
                }
            ),
            ProductMainImageCardinalityException cardinality => (
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["mainImage"] = [cardinality.Message]
                })
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Invalid Product Main Image",
                    Detail = cardinality.Message,
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

        _logger.LogError(exception, "An exception occurred: {ExceptionType}", exception.GetType().Name);

        context.Response.ContentType = "application/problem+json";
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
