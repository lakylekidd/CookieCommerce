namespace CookieCommerce.ProductDomain.Application.Products.Integration;

/// <summary>
/// Integration event published when a product is deactivated.
/// Other services can react to this event to handle product deactivation.
/// </summary>
public record ProductDeactivatedIntegrationEvent(Guid ProductId);
