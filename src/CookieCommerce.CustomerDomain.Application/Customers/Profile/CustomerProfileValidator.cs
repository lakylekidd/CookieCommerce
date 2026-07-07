using System.Text.RegularExpressions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;

namespace CookieCommerce.CustomerDomain.Application.Customers.Profile;

/// <summary>
/// Result returned by <see cref="CustomerProfileValidator"/>.
/// Accumulates per-field validation errors; IsValid is true when Errors is empty.
/// </summary>
public sealed class CustomerProfileValidationResult
{
    private readonly Dictionary<string, List<string>> _errors = new();

    /// <summary>True when there are no field errors.</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>Field-keyed error messages. Keys use camelCase to match JSON property names.</summary>
    public IReadOnlyDictionary<string, string[]> Errors =>
        _errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());

    internal void AddError(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var list))
        {
            list = new List<string>();
            _errors[field] = list;
        }

        list.Add(message);
    }
}

/// <summary>
/// Validates plaintext profile fields before they are encrypted and forwarded to the aggregate.
/// All boundary rules that require plaintext (length, format, enum, date range) live here.
/// Domain-level guards (encrypted-value presence checks) remain on the aggregate.
/// </summary>
public sealed class CustomerProfileValidator
{
    private const int MaxNameLength = 100;

    // RFC 5322-inspired minimal email check – rejects no-at, no-dot-in-domain, leading/trailing spaces.
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Validates the profile update command's plaintext fields.
    /// Returns a <see cref="CustomerProfileValidationResult"/> with any field errors.
    /// </summary>
    public CustomerProfileValidationResult Validate(
        string firstName,
        string lastName,
        string email,
        CustomerProfileGender? gender,
        DateOnly? dateOfBirth)
    {
        var result = new CustomerProfileValidationResult();

        ValidateFirstName(firstName, result);
        ValidateLastName(lastName, result);
        ValidateEmail(email, result);
        ValidateGender(gender, result);
        ValidateDateOfBirth(dateOfBirth, result);

        return result;
    }

    // ─── Field rules ──────────────────────────────────────────────────────────

    private static void ValidateFirstName(string firstName, CustomerProfileValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            result.AddError("firstName", "First name is required.");
            return;
        }

        if (firstName.Trim().Length > MaxNameLength)
            result.AddError("firstName", $"First name must not exceed {MaxNameLength} characters.");
    }

    private static void ValidateLastName(string lastName, CustomerProfileValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            result.AddError("lastName", "Last name is required.");
            return;
        }

        if (lastName.Trim().Length > MaxNameLength)
            result.AddError("lastName", $"Last name must not exceed {MaxNameLength} characters.");
    }

    private static void ValidateEmail(string email, CustomerProfileValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            result.AddError("email", "Email address is required.");
            return;
        }

        if (!EmailRegex.IsMatch(email.Trim()))
            result.AddError("email", "Email address is not valid.");
    }

    private static void ValidateGender(CustomerProfileGender? gender, CustomerProfileValidationResult result)
    {
        if (gender.HasValue && !Enum.IsDefined(gender.Value))
            result.AddError("gender", "The selected gender is not supported.");
    }

    private static void ValidateDateOfBirth(DateOnly? dateOfBirth, CustomerProfileValidationResult result)
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            result.AddError("dateOfBirth", "Date of birth must be in the past.");
    }
}
