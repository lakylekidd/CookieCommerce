namespace CookieCommerce.CustomerDomain.Domain.Exceptions;

public class CustomerProfileValidationException : Exception
{
    /// <summary>
    /// Field-keyed validation errors (camelCase keys). Empty when the exception carries
    /// only a top-level message without per-field detail.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }

    /// <summary>Single-message validation failure (no field-level breakdown).</summary>
    public CustomerProfileValidationException(string message) : base(message)
    {
        FieldErrors = new Dictionary<string, string[]>();
    }

    /// <summary>Field-level validation failure (carries per-field error messages).</summary>
    public CustomerProfileValidationException(
        IReadOnlyDictionary<string, string[]> fieldErrors,
        string message = "Profile validation failed.")
        : base(message)
    {
        FieldErrors = fieldErrors;
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
