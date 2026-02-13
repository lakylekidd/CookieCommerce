using CookieCommerce.ProductDomain.Domain.Aggregates;
using CookieCommerce.ProductDomain.Domain.Exceptions;
using FluentAssertions;

namespace CookieCommerce.ProductDomain.Tests.DomainTests;

public class ProductAggregateTests
{
    [Fact]
    public void Create_ShouldThrow_WhenNameIsEmpty()
    {
        var agg = new ProductAggregate();

        Action act = () => agg.Create(Guid.NewGuid(), "", "description", 10.99m);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenDescriptionIsEmpty()
    {
        var agg = new ProductAggregate();

        Action act = () => agg.Create(Guid.NewGuid(), "Product Name", "", 10.99m);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("*description*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenPriceIsZero()
    {
        var agg = new ProductAggregate();
        var id = Guid.NewGuid();

        Action act = () => agg.Create(id, "Product Name", "Description", 0m);
        
        act.Should().Throw<InvalidPriceException>()
            .Which.ProductId.Should().Be(id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenPriceIsNegative()
    {
        var agg = new ProductAggregate();
        var id = Guid.NewGuid();

        Action act = () => agg.Create(id, "Product Name", "Description", -5.99m);
        
        act.Should().Throw<InvalidPriceException>()
            .Which.Price.Should().Be(-5.99m);
    }

    [Fact]
    public void Create_ShouldSetAllProperties_WhenValidDataProvided()
    {
        var agg = new ProductAggregate();
        var id = Guid.NewGuid();
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;

        agg.Create(id, name, description, price);

        agg.Id.Should().Be(id);
        agg.Name.Should().Be(name);
        agg.Description.Should().Be(description);
        agg.Price.Should().Be(price);
        agg.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Rename_ShouldUpdateName_WhenProductIsActive()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Old Name", "Description", 10.99m);

        agg.Rename("New Name");

        agg.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_ShouldThrow_WhenProductIsDeactivated()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);
        agg.Deactivate();

        Action act = () => agg.Rename("New Name");

        act.Should().Throw<CannotRenameDeactivatedProductException>()
            .Which.NewName.Should().Be("New Name");
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsEmpty()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);

        Action act = () => agg.Rename("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePrice_WhenProductIsActive()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);

        agg.ChangePrice(15.99m);

        agg.Price.Should().Be(15.99m);
    }

    [Fact]
    public void ChangePrice_ShouldThrow_WhenNewPriceIsZero()
    {
        var agg = new ProductAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "Product Name", "Description", 10.99m);

        Action act = () => agg.ChangePrice(0m);

        act.Should().Throw<InvalidPriceException>()
            .Which.ProductId.Should().Be(id);
    }

    [Fact]
    public void ChangePrice_ShouldThrow_WhenNewPriceIsNegative()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);

        Action act = () => agg.ChangePrice(-5.99m);

        act.Should().Throw<InvalidPriceException>()
            .Which.Price.Should().Be(-5.99m);
    }

    [Fact]
    public void ChangePrice_ShouldThrow_WhenProductIsDeactivated()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);
        agg.Deactivate();

        Action act = () => agg.ChangePrice(15.99m);

        act.Should().Throw<CannotChangePriceOfDeactivatedProductException>()
            .Which.NewPrice.Should().Be(15.99m);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);

        agg.Deactivate();

        agg.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldBeIdempotent_WhenCalledMultipleTimes()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product Name", "Description", 10.99m);

        agg.Deactivate();
        agg.Deactivate();
        agg.Deactivate();

        agg.IsActive.Should().BeFalse();
    }

    [Fact]
    public void EventSourcing_ShouldReplayEventsCorrectly()
    {
        // Arrange: Create aggregate and perform operations
        var agg = new ProductAggregate();
        var id = Guid.NewGuid();
        agg.Create(id, "Original Name", "Original Description", 10.99m);
        agg.Rename("Updated Name");
        agg.ChangePrice(15.99m);
        agg.Deactivate();

        var events = agg.DomainEvents.ToList();

        // Act: Replay events on a new aggregate
        var replayedAgg = new ProductAggregate();
        foreach (var evt in events)
        {
            var whenMethod = typeof(ProductAggregate).GetMethod("When",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            whenMethod?.Invoke(replayedAgg, new[] { evt });
        }

        // Assert: State should match original
        replayedAgg.Id.Should().Be(agg.Id);
        replayedAgg.Name.Should().Be(agg.Name);
        replayedAgg.Description.Should().Be(agg.Description);
        replayedAgg.Price.Should().Be(agg.Price);
        replayedAgg.IsActive.Should().Be(agg.IsActive);
    }

    [Fact]
    public void CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var agg = new ProductAggregate();
        var id = Guid.NewGuid();

        // Act: Create product
        agg.Create(id, "Widget", "A useful widget", 19.99m);

        agg.Id.Should().Be(id);
        agg.Name.Should().Be("Widget");
        agg.IsActive.Should().BeTrue();

        // Act: Rename product
        agg.Rename("Super Widget");
        agg.Name.Should().Be("Super Widget");

        // Act: Change price
        agg.ChangePrice(24.99m);
        agg.Price.Should().Be(24.99m);

        // Act: Deactivate
        agg.Deactivate();
        agg.IsActive.Should().BeFalse();

        // Assert: Cannot rename or change price after deactivation
        Action renameAct = () => agg.Rename("New Name");
        renameAct.Should().Throw<CannotRenameDeactivatedProductException>();

        Action priceAct = () => agg.ChangePrice(29.99m);
        priceAct.Should().Throw<CannotChangePriceOfDeactivatedProductException>();
    }

    [Fact]
    public void DomainEvents_ShouldBeTracked()
    {
        var agg = new ProductAggregate();
        agg.Create(Guid.NewGuid(), "Product", "Description", 10.99m);
        agg.Rename("New Product");
        agg.ChangePrice(15.99m);

        agg.DomainEvents.Should().HaveCount(3);
        agg.DomainEvents.Should().ContainItemsAssignableTo<object>();
    }
}
