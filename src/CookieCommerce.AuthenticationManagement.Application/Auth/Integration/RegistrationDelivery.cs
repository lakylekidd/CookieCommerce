namespace CookieCommerce.AuthenticationManagement.Application.Auth.Integration;

public sealed class RegistrationDelivery
{
    public Guid Id { get; set; }
    public UserRegisteredIntegrationEvent Message { get; set; } = new();
    public DateTimeOffset? PublishedAt { get; set; }
}
