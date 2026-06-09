namespace CookieCommerce.AuthenticationManagement.Domain.Exceptions;

public class AuthUserNotFoundException(Guid userId) : Exception($"Auth user {userId} not found.");
