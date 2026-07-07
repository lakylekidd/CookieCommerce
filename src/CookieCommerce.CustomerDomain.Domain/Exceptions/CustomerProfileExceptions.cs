namespace CookieCommerce.CustomerDomain.Domain.Exceptions;

public class CustomerProfileValidationException : Exception
{
    public CustomerProfileValidationException(string message) : base(message)
    {
    }
}

public class DuplicateCustomerEmailException : Exception
{
    public DuplicateCustomerEmailException(string message = "Email address is already in use.") : base(message)
    {
    }
}

public class MissingCustomerProfileException : Exception
{
    public MissingCustomerProfileException(Guid customerId) : base($"Customer profile '{customerId}' was not found.")
    {
    }
}

public class InvalidEmailVerificationTokenException : Exception
{
    public InvalidEmailVerificationTokenException(string message = "Email verification token is invalid.") : base(message)
    {
    }
}

public class ExpiredEmailVerificationTokenException : Exception
{
    public ExpiredEmailVerificationTokenException() : base("Email verification token has expired.")
    {
    }
}

public class UnauthorizedCustomerProfileAccessException : Exception
{
    public UnauthorizedCustomerProfileAccessException() : base("The authenticated customer cannot access this profile.")
    {
    }
}
