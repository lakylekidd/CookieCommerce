namespace CookieCommerce.OrderDomain.Application.Customers.Integration;

/// <summary>
/// Integration event received when a customer is deleted/forgotten.
/// Mirrors CookieCommerce.CustomerDomain.Application.Customers.Integration.CustomerDeletedIntegrationEvent
/// </summary>
public record CustomerDeletedIntegrationEvent(Guid CustomerId);
