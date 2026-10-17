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


}
