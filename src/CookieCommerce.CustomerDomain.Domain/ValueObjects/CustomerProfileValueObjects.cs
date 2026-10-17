namespace CookieCommerce.CustomerDomain.Domain.ValueObjects;

public enum CustomerProfileGender
{
    Male = 1,
    Female = 2,
    PreferNotToDisclose = 3
}

public readonly record struct CustomerDateOfBirth(DateOnly Value)
{
    public static CustomerDateOfBirth Create(DateOnly value, DateOnly today)
    {
        if (value >= today)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Date of birth must be in the past.");
        }

        return new CustomerDateOfBirth(value);
    }
}
