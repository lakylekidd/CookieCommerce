using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private IDocumentStore _store = null!;
    private IDocumentSession _session = null!;
    private IQuerySession _query = null!;

    public CustomerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        // Get the document store from DI
        _store = _factory.Services.GetRequiredService<IDocumentStore>();
        
        // Create sessions for the test
        _session = _store.LightweightSession();
        _query = _store.QuerySession();
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up sessions
        _session?.Dispose();
        _query?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreatingCustomer_ShouldProjectReadModel()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Create encryption key and encrypt data for the customer
        using var scope = _factory.Services.CreateScope();
        var keyStore = scope.ServiceProvider.GetRequiredService<CookieCommerce.CustomerDomain.Application.Security.ICustomerKeyStore>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<CookieCommerce.CustomerDomain.Application.Security.IEncryptionService>();
        
        var customerKey = await keyStore.CreateKeyForCustomerAsync(id, CancellationToken.None);
        
        var evt = new CustomerCreated
        {
            CustomerId = id,
            EncryptedFirstName = encryptionService.Encrypt("John", customerKey),
            EncryptedLastName = encryptionService.Encrypt("Doe", customerKey),
            EncryptedGender = encryptionService.Encrypt("Male", customerKey),
            EncryptedEmail = encryptionService.Encrypt("john.doe@example.com", customerKey),
            EncryptedDateOfBirth = encryptionService.Encrypt("1990-01-01", customerKey)
        };

        // Act 
        _session.Events.StartStream<CustomerAggregate>(id, evt);
        await _session.SaveChangesAsync();

        // Give projections a moment to process (if async projections are enabled)
        await Task.Delay(100);

        var model = await _query.LoadAsync<CustomerReadModel>(id);

        // Assert
        model.Should().NotBeNull();
        model!.Id.Should().Be(id);
        model.Email.Should().Be("john.doe@example.com");
        model.FirstName.Should().Be("John");
        model.LastName.Should().Be("Doe");
    }
}
