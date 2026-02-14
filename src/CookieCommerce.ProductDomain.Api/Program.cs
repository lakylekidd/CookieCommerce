using CookieCommerce.CookieMassTransit;
using CookieCommerce.ProductDomain.Api.Middleware;
using CookieCommerce.ProductDomain.Application;
using CookieCommerce.ProductDomain.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCookieMassTransit(builder.Configuration);
builder.Services.AddMartenConfiguration(builder.Configuration);
builder.Services.AddOutboxPattern();
builder.Services.AddApplication();

var app = builder.Build();

app.MapDefaultEndpoints();

// Add domain exception handling middleware
app.UseDomainExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program() { }
