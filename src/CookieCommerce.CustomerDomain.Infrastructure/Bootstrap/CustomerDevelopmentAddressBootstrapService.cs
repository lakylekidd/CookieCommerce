using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.DevBootstrap;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CookieCommerce.CustomerDomain.Infrastructure.Bootstrap;

internal sealed class CustomerDevelopmentAddressBootstrapService(
    IServiceScopeFactory scopeFactory,
    ILogger<CustomerDevelopmentAddressBootstrapService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Development customer address bootstrap starting");

        foreach (var customer in DevelopmentBootstrapData.Customers)
        {
            await EnsureCustomerAddressesWithRetryAsync(customer, stoppingToken);
        }

        logger.LogInformation("Development customer address bootstrap complete");
    }

    private async Task EnsureCustomerAddressesWithRetryAsync(
        DevelopmentBootstrapCustomer customer,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 15; attempt++)
        {
            try
            {
                if (await TryEnsureCustomerAddressesAsync(customer, cancellationToken))
                    return;

                logger.LogInformation(
                    "Development bootstrap customer '{Email}' is not available yet on attempt {Attempt}; retrying",
                    customer.Email,
                    attempt);
            }
            catch (Exception ex) when (attempt < 15)
            {
                logger.LogWarning(
                    ex,
                    "Development bootstrap could not ensure addresses for '{Email}' on attempt {Attempt}; retrying",
                    customer.Email,
                    attempt);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }

        if (!await TryEnsureCustomerAddressesAsync(customer, cancellationToken))
        {
            logger.LogWarning("Development bootstrap customer '{Email}' was not found; skipping address seed", customer.Email);
        }
    }

    private async Task<bool> TryEnsureCustomerAddressesAsync(
        DevelopmentBootstrapCustomer customer,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var querySession = scope.ServiceProvider.GetRequiredService<IQuerySession>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        var customerReadModel = await querySession.Query<CustomerReadModel>()
            .FirstOrDefaultAsync(x => x.Email == customer.Email, cancellationToken);

        if (customerReadModel is null)
            return false;

        var existingLabels = customerReadModel.Addresses
            .Select(x => x.Label)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var createdAtUtc = DateTimeOffset.UtcNow;
        var addressIndex = 0;
        foreach (var address in customer.Addresses)
        {
            if (existingLabels.Contains(address.Label))
            {
                logger.LogInformation(
                    "Seed address '{Label}' for customer '{Email}' already exists — skipping",
                    address.Label,
                    customer.Email);
                continue;
            }

            try
            {
                await mediator.Send(new AddCustomerAddress(
                    customerReadModel.Id,
                    Guid.CreateVersion7(),
                    address.Icon,
                    address.Label,
                    customer.FirstName,
                    customer.LastName,
                    address.Street,
                    address.HouseNumber,
                    address.HouseNumberAddition,
                    address.City,
                    address.PostalCode,
                    address.Province,
                    address.Country,
                    address.Phone,
                    address.IsDefault,
                    createdAtUtc.AddSeconds(addressIndex++)), cancellationToken);

                logger.LogInformation(
                    "Seed address '{Label}' for customer '{Email}' created",
                    address.Label,
                    customer.Email);
            }
            catch (DuplicateCustomerAddressLabelException)
            {
                logger.LogInformation(
                    "Seed address '{Label}' for customer '{Email}' already exists — skipping",
                    address.Label,
                    customer.Email);
            }
        }

        return true;
    }
}
