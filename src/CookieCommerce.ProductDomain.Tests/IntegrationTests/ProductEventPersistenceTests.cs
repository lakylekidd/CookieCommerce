using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.DomainEvents;
using FluentAssertions;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace CookieCommerce.ProductDomain.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Product event persistence with Marten.
/// Uses PostgreSQL testcontainer for isolated testing.
/// </summary>
public class ProductEventPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private IServiceProvider _serviceProvider = default!;
    private IDocumentStore _documentStore = default!;

    public ProductEventPersistenceTests()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("product_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var services = new ServiceCollection();

        // Configure Marten with testcontainer connection string
        services.AddMarten(opts =>
        {
            opts.Connection(_postgresContainer.GetConnectionString());

            // Configure event metadata
            opts.Events.MetadataConfig.CausationIdEnabled = true;
            opts.Events.MetadataConfig.CorrelationIdEnabled = true;
            opts.Events.MetadataConfig.HeadersEnabled = true;

            // Auto-create schema
            opts.AutoCreateSchemaObjects = JasperFx.AutoCreate.All;
        });

        _serviceProvider = services.BuildServiceProvider();
        _documentStore = _serviceProvider.GetRequiredService<IDocumentStore>();

        // Ensure schema is created
        await _documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task ProductCreated_ShouldBePersisted_WithMetadata()
    {
        // Arrange
        var productId = Guid.CreateVersion7();
        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Test Description", 99.99m);

        await using var session = _documentStore.LightweightSession();

        // Act
        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync();

        // Assert - Verify event is persisted
        var events = await session.Events.FetchStreamAsync(productId);
        events.Should().HaveCount(1);
        events[0].Data.Should().BeOfType<ProductCreated>();

        var productCreated = events[0].Data as ProductCreated;
        productCreated!.ProductId.Should().Be(productId);
        productCreated.Name.Should().Be("Test Product");
        productCreated.Description.Should().Be("Test Description");
        productCreated.Price.Should().Be(99.99m);

        // Assert - Verify event metadata
        events[0].Id.Should().NotBeEmpty();
        events[0].Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        events[0].StreamId.Should().Be(productId);
        events[0].Version.Should().Be(1);
    }

    [Fact]
    public async Task ProductRenamed_ShouldBePersisted_WithCorrectVersion()
    {
        // Arrange
        var productId = Guid.CreateVersion7();
        var product = new ProductAggregate();
        product.Create(productId, "Original Name", "Description", 50.00m);
        product.Rename("New Name");

        await using var session = _documentStore.LightweightSession();

        // Act
        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync();

        // Assert
        var events = await session.Events.FetchStreamAsync(productId);
        events.Should().HaveCount(2);

        events[0].Data.Should().BeOfType<ProductCreated>();
        events[1].Data.Should().BeOfType<ProductRenamed>();

        var renamed = events[1].Data as ProductRenamed;
        renamed!.ProductId.Should().Be(productId);
        renamed.NewName.Should().Be("New Name");

        // Verify version sequencing
        events[0].Version.Should().Be(1);
        events[1].Version.Should().Be(2);
    }

    [Fact]
    public async Task ProductPriceChanged_ShouldBePersisted_WithPriceHistory()
    {
        // Arrange
        var productId = Guid.CreateVersion7();
        var product = new ProductAggregate();
        product.Create(productId, "Product", "Description", 100.00m);
        product.ChangePrice(150.00m);

        await using var session = _documentStore.LightweightSession();

        // Act
        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync();

        // Assert
        var events = await session.Events.FetchStreamAsync(productId);
        events.Should().HaveCount(2);

        var priceChanged = events[1].Data as ProductPriceChanged;
        priceChanged!.ProductId.Should().Be(productId);
        priceChanged.OldPrice.Should().Be(100.00m);
        priceChanged.NewPrice.Should().Be(150.00m);
    }

    [Fact]
    public async Task ProductDeactivated_ShouldBePersisted()
    {
        // Arrange
        var productId = Guid.CreateVersion7();
        var product = new ProductAggregate();
        product.Create(productId, "Product", "Description", 75.00m);
        product.Deactivate();

        await using var session = _documentStore.LightweightSession();

        // Act
        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync();

        // Assert
        var events = await session.Events.FetchStreamAsync(productId);
        events.Should().HaveCount(2);

        events[1].Data.Should().BeOfType<ProductDeactivated>();
        var deactivated = events[1].Data as ProductDeactivated;
        deactivated!.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task MultipleProducts_ShouldBePersisted_InSeparateStreams()
    {
        // Arrange
        var productId1 = Guid.CreateVersion7();
        var productId2 = Guid.CreateVersion7();

        var product1 = new ProductAggregate();
        product1.Create(productId1, "Product 1", "Description 1", 10.00m);

        var product2 = new ProductAggregate();
        product2.Create(productId2, "Product 2", "Description 2", 20.00m);

        await using var session = _documentStore.LightweightSession();

        // Act
        session.Events.StartStream<ProductAggregate>(productId1, product1.DomainEvents);
        session.Events.StartStream<ProductAggregate>(productId2, product2.DomainEvents);
        await session.SaveChangesAsync();

        // Assert
        var events1 = await session.Events.FetchStreamAsync(productId1);
        var events2 = await session.Events.FetchStreamAsync(productId2);

        events1.Should().HaveCount(1);
        events2.Should().HaveCount(1);

        var created1 = events1[0].Data as ProductCreated;
        var created2 = events2[0].Data as ProductCreated;

        created1!.Name.Should().Be("Product 1");
        created2!.Name.Should().Be("Product 2");
    }

    [Fact]
    public async Task ProductAggregate_CanBeRehydrated_FromEvents()
    {
        // Arrange
        var productId = Guid.CreateVersion7();
        var product = new ProductAggregate();
        product.Create(productId, "Test Product", "Description", 100.00m);
        product.Rename("Updated Name");
        product.ChangePrice(150.00m);

        await using var session = _documentStore.LightweightSession();
        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync();

        // Act - Rehydrate aggregate from events
        var rehydratedProduct = await session.Events.AggregateStreamAsync<ProductAggregate>(productId);

        // Assert
        rehydratedProduct.Should().NotBeNull();
        rehydratedProduct!.Id.Should().Be(productId);
        rehydratedProduct.Name.Should().Be("Updated Name");
        rehydratedProduct.Price.Should().Be(150.00m);
        rehydratedProduct.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task EventMetadata_ShouldBeConfigured_WithTimestamps()
    {
        // Arrange
        var productId = Guid.CreateVersion7();
        var product = new ProductAggregate();
        product.Create(productId, "Metadata Test", "Description", 50.00m);

        await using var session = _documentStore.LightweightSession();

        // Act
        var beforeSave = DateTimeOffset.UtcNow;
        session.Events.StartStream<ProductAggregate>(productId, product.DomainEvents);
        await session.SaveChangesAsync();
        var afterSave = DateTimeOffset.UtcNow;

        // Assert
        var events = await session.Events.FetchStreamAsync(productId);
        var @event = events[0];

        // Verify timestamp is within reasonable range
        @event.Timestamp.Should().BeOnOrAfter(beforeSave);
        @event.Timestamp.Should().BeOnOrBefore(afterSave);

        // Verify event ID is generated
        @event.Id.Should().NotBeEmpty();

        // Verify stream information
        @event.StreamId.Should().Be(productId);
        @event.Version.Should().Be(1);
    }
}
