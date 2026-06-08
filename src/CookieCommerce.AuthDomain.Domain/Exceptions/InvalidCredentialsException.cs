namespace CookieCommerce.AuthDomain.Domain.Exceptions;

public class InvalidCredentialsException(string message) : Exception(message);
