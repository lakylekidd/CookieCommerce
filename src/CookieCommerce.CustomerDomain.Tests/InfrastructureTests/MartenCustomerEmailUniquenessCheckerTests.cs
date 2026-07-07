using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Infrastructure.Persistence;
using FluentAssertions;
using Marten;
using Testcontainers.PostgreSql;

namespace CookieCommerce.CustomerDomain.Tests.InfrastructureTests;

public sealed class MartenCustomerEmailUniquenessCheckerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    private IDocumentStore _store = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _store = DocumentStore.For(options =>
        {
            options.Connection(_postgres.GetConnectionString());
            options.Schema.For<CustomerReadModel>().Identity(customer => customer.Id);
        });
    }

    public async Task DisposeAsync()
    {
        _store.Dispose();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CheckAsync_ReturnsVerifiedEmailConflict_GivenAnotherCustomerHasVerifiedEmail()
    {
        var currentCustomerId = Guid.NewGuid();
        await StoreCustomerAsync(new CustomerReadModel
        {
            Id = Guid.NewGuid(),
            Email = "taken@example.com",
            FirstName = "Taken",
            LastName = "Verified"
        });

        await using var session = _store.QuerySession();
        var checker = new MartenCustomerEmailUniquenessChecker(session);

        var result = await checker.CheckAsync(" TAKEN@example.com ", currentCustomerId, CancellationToken.None);

        result.IsAvailable.Should().BeFalse();
        result.ConflictKind.Should().Be(CustomerEmailConflictKind.VerifiedEmail);
    }

    [Fact]
    public async Task CheckAsync_ReturnsPendingEmailConflict_GivenAnotherCustomerHasPendingEmail()
    {
        var currentCustomerId = Guid.NewGuid();
        await StoreCustomerAsync(new CustomerReadModel
        {
            Id = Guid.NewGuid(),
            Email = "verified@example.com",
            PendingEmail = "pending@example.com",
            EmailVerificationStatus = "PendingVerification",
            FirstName = "Taken",
            LastName = "Pending"
        });

        await using var session = _store.QuerySession();
        var checker = new MartenCustomerEmailUniquenessChecker(session);

        var result = await checker.CheckAsync("pending@example.com", currentCustomerId, CancellationToken.None);

        result.IsAvailable.Should().BeFalse();
        result.ConflictKind.Should().Be(CustomerEmailConflictKind.PendingEmail);
    }

    [Fact]
    public async Task EnsureEmailAvailableForPendingChangeAsync_ThrowsDuplicateEmailException_GivenDuplicatePendingEmail()
    {
        await StoreCustomerAsync(new CustomerReadModel
        {
            Id = Guid.NewGuid(),
            Email = "verified@example.com",
            PendingEmail = "pending@example.com",
            EmailVerificationStatus = "PendingVerification",
            FirstName = "Taken",
            LastName = "Pending"
        });

        await using var session = _store.QuerySession();
        var service = new CustomerEmailUniquenessService(new MartenCustomerEmailUniquenessChecker(session));

        var act = () => service.EnsureEmailAvailableForPendingChangeAsync("pending@example.com", Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateCustomerEmailException>();
    }

    [Fact]
    public async Task CheckAsync_ReturnsAvailable_GivenEmailBelongsOnlyToCurrentCustomer()
    {
        var currentCustomerId = Guid.NewGuid();
        await StoreCustomerAsync(new CustomerReadModel
        {
            Id = currentCustomerId,
            Email = "current@example.com",
            PendingEmail = "current.pending@example.com",
            EmailVerificationStatus = "PendingVerification",
            FirstName = "Current",
            LastName = "Customer"
        });

        await using var session = _store.QuerySession();
        var checker = new MartenCustomerEmailUniquenessChecker(session);

        var result = await checker.CheckAsync("current.pending@example.com", currentCustomerId, CancellationToken.None);

        result.IsAvailable.Should().BeTrue();
        result.ConflictKind.Should().Be(CustomerEmailConflictKind.None);
    }

    private async Task StoreCustomerAsync(CustomerReadModel customer)
    {
        await using var session = _store.LightweightSession();
        session.Store(customer);
        await session.SaveChangesAsync();
    }
}
