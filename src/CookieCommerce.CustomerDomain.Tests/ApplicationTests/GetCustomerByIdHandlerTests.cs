using CookieCommerce.CustomerDomain.Application.Customers.Handlers;
using CookieCommerce.CustomerDomain.Application.Customers.Queries;
using CookieCommerce.CustomerDomain.Application.Customers.ReadModels;
using CookieCommerce.CustomerDomain.Application.Security;
using Marten;
using NSubstitute;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

public class GetCustomerByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCustomerReadModel_WhenFound()
    {
        var query = new GetCustomerById(Guid.NewGuid());
        var session = Substitute.For<IQuerySession>();

        var expected = new CustomerReadModel { Id = query.CustomerId, Email = "a@b.com", FirstName = "John", LastName = "Doe", Gender = Domain.ValueObjects.CustomerProfileGender.Male };

        session.LoadAsync<CustomerReadModel>(query.CustomerId).Returns(Task.FromResult(expected));

        var handler = new GetCustomerByIdHandler(session);

        var result = await handler.Handle(query, default);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        var query = new GetCustomerById(Guid.NewGuid());
        var session = Substitute.For<IQuerySession>();

        session.LoadAsync<CustomerReadModel>(query.CustomerId).Returns(Task.FromResult<CustomerReadModel?>(null));

        var handler = new GetCustomerByIdHandler(session);

        var result = await handler.Handle(query, default);

        result.Should().BeNull();
    }
}
