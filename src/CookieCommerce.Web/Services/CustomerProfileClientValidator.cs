using System.Text.RegularExpressions;
using CookieCommerce.Web.Models.Account;

namespace CookieCommerce.Web.Services;

/// <summary>
/// Mirrors CustomerDomain's CustomerProfileValidator so the profile edit UI can
/// reject invalid input before sending a PATCH request.
/// </summary>
public static partial class CustomerProfileClientValidator
{
    public const int MaxNameLength = 100;

    private static readonly HashSet<string> SupportedGenders =
    [
        "Male",
        "Female",
        "PreferNotToDisclose"
    ];

    public static Dictionary<string, string[]> Validate(UserProfileDto profile)
    {
        var errors = new Dictionary<string, List<string>>();

        ValidateFirstName(profile.FirstName, errors);
        ValidateLastName(profile.LastName, errors);
        ValidateEmail(profile.Email, errors);
        ValidateGender(profile.Gender, errors);
        ValidateDateOfBirth(profile.DateOfBirth, errors);

        return errors.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
    }

    private static void ValidateFirstName(string firstName, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            AddError(errors, "firstName", "First name is required.");
            return;
        }

        if (firstName.Trim().Length > MaxNameLength)
            AddError(errors, "firstName", $"First name must not exceed {MaxNameLength} characters.");
    }

    private static void ValidateLastName(string lastName, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            AddError(errors, "lastName", "Last name is required.");
            return;
        }

        if (lastName.Trim().Length > MaxNameLength)
            AddError(errors, "lastName", $"Last name must not exceed {MaxNameLength} characters.");
    }

    private static void ValidateEmail(string email, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, "email", "Email address is required.");
            return;
        }

        if (!EmailRegex().IsMatch(email.Trim()))
            AddError(errors, "email", "Email address is not valid.");
    }

    private static void ValidateGender(string? gender, Dictionary<string, List<string>> errors)
    {
        if (!string.IsNullOrWhiteSpace(gender) && !SupportedGenders.Contains(gender))
            AddError(errors, "gender", "The selected gender is not supported.");
    }

    private static void ValidateDateOfBirth(DateOnly? dateOfBirth, Dictionary<string, List<string>> errors)
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            AddError(errors, "dateOfBirth", "Date of birth must be in the past.");
    }

    private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var messages))
        {
            messages = [];
            errors[field] = messages;
        }

        messages.Add(message);
    }

    // Keep this pattern identical to CustomerDomain.Application.Customers.Profile.CustomerProfileValidator.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, 250)]
    private static partial Regex EmailRegex();
}
