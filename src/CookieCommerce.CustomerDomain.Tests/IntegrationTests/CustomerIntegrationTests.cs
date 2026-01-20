using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerIntegrationTests : IAsyncLifetime
{
    private IHost _host;
    private IQuerySession _query;
    private IDocumentSession _session;

    [Fact]
    public async Task CreatingCustomer_ShouldProjectReadModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        var evt = new CustomerCreated
        {
            CustomerId = id,
            EncryptedFirstName = "EncryptedFirstName",
            EncryptedLastName = "EncryptedLastName",
            EncryptedGender = "EncryptedGender",
            EncryptedEmail = "EncryptedEmail",
            EncryptedDateOfBirth = "EncryptedDateOfBirth"
        };

        // Act 
        _session.Events.StartStream<CustomerAggregate>(id, evt);
        await _session.SaveChangesAsync();

       var model = await _query.LoadAsync<CustomerReadModel>(id);

        // Assert
        model.Should().NotBeNull();
    }

    public async Task InitializeAsync()
    {
        _host = await TestHostFactory.BuildAsync();
        _query = _host.Services.GetRequiredService<IQuerySession>();
        _session = _host.Services.GetRequiredService<IDocumentSession>();
    }
    public Task DisposeAsync()
    {
        _host.Dispose();
        return Task.CompletedTask;
    }
}
