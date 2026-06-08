namespace CookieCommerce.AuthDomain.Domain.Exceptions;

public class AuthUserNotFoundException(Guid userId) : Exception($"Auth user {userId} not found.");
