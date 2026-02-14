using CookieCommerce.DomainBuildingBlocks;
using CookieCommerce.ProductDomain.Application.Products.EventHandlers;
using CookieCommerce.ProductDomain.Application.Products.Integration;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace CookieCommerce.ProductDomain.Tests.OutboxTests;

public class ProductDomainEventHandlerTests
{
    [Fact]
    public async Task Handle_ProductCreated_ShouldPublishProductSnapshotPublished()
    {
        // Arrange
        var publisher = Substitute.For<IIntegrationEventPublisher>();
        var session = Substitute.For<IDocumentSession>();
        var handler = new ProductDomainEventHandler(publisher, session);

        var domainEvent = new ProductCreated
        {
            ProductId = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m
        };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await publisher.Received(1).PublishAsync(
            Arg.Is<ProductSnapshotPublished>(e =>
                e.ProductId == domainEvent.ProductId &&
                e.Name == domainEvent.Name &&
                e.Description == domainEvent.Description &&
                e.Price == domainEvent.Price &&
                e.IsActive == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductDeactivated_ShouldPublishProductDeactivatedIntegrationEvent()
    {
        // Arrange
        var publisher = Substitute.For<IIntegrationEventPublisher>();
        var session = Substitute.For<IDocumentSession>();
        var handler = new ProductDomainEventHandler(publisher, session);

        var productId = Guid.NewGuid();
        var domainEvent = new ProductDeactivated(productId);

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await publisher.Received(1).PublishAsync(
            Arg.Is<ProductDeactivatedIntegrationEvent>(e => e.ProductId == productId),
            Arg.Any<CancellationToken>());
    }
}
