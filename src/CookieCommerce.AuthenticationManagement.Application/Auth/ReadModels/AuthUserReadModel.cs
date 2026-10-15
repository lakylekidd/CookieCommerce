namespace CookieCommerce.AuthenticationManagement.Application.Auth.ReadModels;

public class AuthUserReadModel
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string KeycloakId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
