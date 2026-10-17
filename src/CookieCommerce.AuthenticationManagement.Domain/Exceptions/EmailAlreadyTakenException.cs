namespace CookieCommerce.AuthenticationManagement.Domain.Exceptions;

public class EmailAlreadyTakenException(string email) : Exception($"Email '{email}' is already registered.");
