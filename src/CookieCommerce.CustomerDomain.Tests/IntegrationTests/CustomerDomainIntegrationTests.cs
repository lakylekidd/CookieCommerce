using System.Linq;
using System.Security.Cryptography;
using CookieCommerce.CustomerDomain.Application.Customers.Commands;
using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.DomainEvents;
using CookieCommerce.CustomerDomain.Infrastructure.Projections;
using CookieCommerce.CustomerDomain.Infrastructure.Security;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.IntegrationTests;

public class CustomerDomainIntegrationTests
{
    private class InMemoryCustomerKeyStore : ICustomerKeyStore
    {
        private readonly Dictionary<Guid, byte[]> _store = new();

        public Task<byte[]> CreateKeyForCustomerAsync(Guid customerId, CancellationToken ct)
        {
            var key = RandomNumberGenerator.GetBytes(32);
            _store[customerId] = key;
            return Task.FromResult(key);
        }

        public Task DeleteKeyAsync(Guid customerId, CancellationToken ct)
        {
            _store.Remove(customerId);
            return Task.CompletedTask;
        }

        public Task<byte[]?> GetKeyAsync(Guid customerId, CancellationToken ct)
        {
            _store.TryGetValue(customerId, out var key);
            return Task.FromResult<byte[]?>(key);
        }
    }

    [Fact]
    public async Task CreateCustomer_HandlerAndProjection_EndToEnd()
    {
        // Arrange - real encryption and in-memory keystore, substitute document session
        var keyStore = new InMemoryCustomerKeyStore();
        var encryption = new EncryptionService();

        var session = Substitute.For<IDocumentSession>();
        // Capture the domain events passed to StartStream
        List<object>? captured = null;
        session.Events.StartStream<CustomerAggregate>(Arg.Any<Guid>(), Arg.Do<IEnumerable<object>>(e => captured = e.ToList()));
        session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var handler = new CreateCustomerHandler(session, keyStore, encryption);

        // Act - create customer
        var customerId = await handler.Handle(
            new CreateCustomer("test@example.com", "John", "Doe", Domain.ValueObjects.Gender.Male, DateTime.Parse("1986-11-18")),
            default);

        // Assert domain events were started
        Assert.NotNull(captured);
        var created = captured!.OfType<CustomerCreated>().Single();
        created.CustomerId.Should().Be(customerId);

        // Now apply projection using the real EncryptionService/keyStore via DI
        var services = new ServiceCollection();
        services.AddSingleton<ICustomerKeyStore>(keyStore);
        services.AddSingleton<IEncryptionService>(encryption);
        var provider = services.BuildServiceProvider();

        var projection = new CustomerProjection(provider);

        var ops = Substitute.For<IDocumentOperations>();

        // Wrap domain event into JasperFx Event wrapper used by projections
        var jEvents = captured.OfType<CustomerCreated>().Select(e => new JasperFx.Events.Event<CustomerCreated>(e)).ToArray();

        await projection.ApplyAsync(ops, jEvents, default);

        // Verify projection stored a read model with decrypted values
        ops.Received().Store(Arg.Is<CustomerReadModel>(m =>
            m.Id == customerId &&
            m.Email == "test@example.com" &&
            m.FirstName == "John" &&
            m.LastName == "Doe" &&
            m.Gender == Domain.ValueObjects.Gender.Male &&
            m.DateOfBirth != null && m.DateOfBirth.Value.Year == 1986
        ));
    }
}
