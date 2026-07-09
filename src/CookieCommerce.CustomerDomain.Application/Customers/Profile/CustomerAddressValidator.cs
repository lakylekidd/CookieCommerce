using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

public sealed class CustomerAddressValidator
{
    private const int MaxShortTextLength = 100;
    private const int MaxLongTextLength = 200;

    public ValidatedCustomerAddress Validate(
        string? icon,
        string? label,
        string? firstName,
        string? lastName,
        string? street,
        string? houseNumber,
        string? houseNumberAddition,
        string? city,
        string? postalCode,
        string? province,
        string? country,
        string? phone,
        bool isDefault,
        IEnumerable<Address> existingAddresses)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var trimmedIcon = TrimOptional(icon);
        var trimmedLabel = Require(label, nameof(label), "Label is required.", MaxShortTextLength, errors);
        var trimmedFirstName = Require(firstName, nameof(firstName), "First name is required.", MaxShortTextLength, errors);
        var trimmedLastName = Require(lastName, nameof(lastName), "Last name is required.", MaxShortTextLength, errors);
        var trimmedStreet = Require(street, nameof(street), "Street is required.", MaxLongTextLength, errors);
        var trimmedHouseNumber = Require(houseNumber, nameof(houseNumber), "House number is required.", MaxShortTextLength, errors);
        var trimmedHouseNumberAddition = TrimOptional(houseNumberAddition);
        var trimmedCity = Require(city, nameof(city), "City is required.", MaxShortTextLength, errors);
        var trimmedPostalCode = Require(postalCode, nameof(postalCode), "Postal code is required.", MaxShortTextLength, errors);
        var trimmedProvince = Require(province, nameof(province), "Province is required.", MaxShortTextLength, errors);
        var trimmedCountry = Require(country, nameof(country), "Country is required.", MaxShortTextLength, errors);
        var trimmedPhone = Require(phone, nameof(phone), "Phone is required.", MaxShortTextLength, errors);

        AddMaxLengthError(trimmedIcon, nameof(icon), MaxShortTextLength, errors);
        AddMaxLengthError(trimmedHouseNumberAddition, nameof(houseNumberAddition), MaxShortTextLength, errors);

        if (errors.Count > 0)
        {
            throw new CustomerAddressValidationException(errors.ToDictionary(x => x.Key, x => x.Value.ToArray()));
        }

        return new ValidatedCustomerAddress(
            trimmedIcon,
            trimmedLabel!,
            trimmedFirstName!,
            trimmedLastName!,
            trimmedStreet!,
            trimmedHouseNumber!,
            trimmedHouseNumberAddition,
            trimmedCity!,
            trimmedPostalCode!,
            trimmedProvince!,
            trimmedCountry!,
            trimmedPhone!,
            isDefault);
    }

    private static string? Require(
        string? value,
        string fieldName,
        string message,
        int maxLength,
        Dictionary<string, List<string>> errors)
    {
        var trimmed = TrimOptional(value);
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            AddError(errors, fieldName, message);
            return trimmed;
        }

        AddMaxLengthError(trimmed, fieldName, maxLength, errors);
        return trimmed;
    }

    private static string? TrimOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void AddMaxLengthError(
        string? value,
        string fieldName,
        int maxLength,
        Dictionary<string, List<string>> errors)
    {
        if (value?.Length > maxLength)
        {
            AddError(errors, fieldName, $"{fieldName} must be {maxLength} characters or fewer.");
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string fieldName, string message)
    {
        if (!errors.TryGetValue(fieldName, out var fieldErrors))
        {
            fieldErrors = [];
            errors[fieldName] = fieldErrors;
        }

        fieldErrors.Add(message);
    }
}

public sealed record ValidatedCustomerAddress(
    string? Icon,
    string Label,
    string FirstName,
    string LastName,
    string Street,
    string HouseNumber,
    string? HouseNumberAddition,
    string City,
    string PostalCode,
    string Province,
    string Country,
    string Phone,
    bool IsDefault);
