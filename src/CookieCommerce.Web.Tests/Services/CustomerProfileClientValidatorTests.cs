using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Services;
using FluentAssertions;

namespace CookieCommerce.Web.Tests.Services;

public class CustomerProfileClientValidatorTests
{
    [Fact]
    public void Validate_BlankRequiredFields_ReturnsBackendMatchingMessages()
    {
        var result = CustomerProfileClientValidator.Validate(new UserProfileDto
        {
            FirstName = " ",
            LastName = "",
            Email = "\t"
        });

        result.Should().ContainKey("firstName");
        result["firstName"].Should().Contain("First name is required.");
        result.Should().ContainKey("lastName");
        result["lastName"].Should().Contain("Last name is required.");
        result.Should().ContainKey("email");
        result["email"].Should().Contain("Email address is required.");
    }

    [Fact]
    public void Validate_TooLongNames_ReturnsBackendMatchingMessages()
    {
        var tooLong = new string('a', CustomerProfileClientValidator.MaxNameLength + 1);

        var result = CustomerProfileClientValidator.Validate(new UserProfileDto
        {
            FirstName = tooLong,
            LastName = tooLong,
            Email = "jamie@example.com"
        });

        result["firstName"].Should().Contain("First name must not exceed 100 characters.");
        result["lastName"].Should().Contain("Last name must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsBackendMatchingMessage()
    {
        var result = CustomerProfileClientValidator.Validate(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "bad-email"
        });

        result.Should().ContainKey("email");
        result["email"].Should().Contain("Email address is not valid.");
    }

    [Fact]
    public void Validate_UnsupportedGender_ReturnsBackendMatchingMessage()
    {
        var result = CustomerProfileClientValidator.Validate(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "jamie@example.com",
            Gender = "Other"
        });

        result.Should().ContainKey("gender");
        result["gender"].Should().Contain("The selected gender is not supported.");
    }

    [Fact]
    public void Validate_TodayOrFutureBirthdate_ReturnsBackendMatchingMessage()
    {
        var result = CustomerProfileClientValidator.Validate(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "jamie@example.com",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        result.Should().ContainKey("dateOfBirth");
        result["dateOfBirth"].Should().Contain("Date of birth must be in the past.");
    }

    [Fact]
    public void Validate_ValidProfile_ReturnsNoErrors()
    {
        var result = CustomerProfileClientValidator.Validate(new UserProfileDto
        {
            FirstName = "Jamie",
            LastName = "Baker",
            Email = "jamie@example.com",
            Gender = "PreferNotToDisclose",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20))
        });

        result.Should().BeEmpty();
    }
}
