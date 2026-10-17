using CookieCommerce.CustomerDomain.Application.Customers.Profile;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace CookieCommerce.CustomerDomain.Tests.ApplicationTests;

/// <summary>
/// US2 – application-layer validation: email format, duplicate verified/pending email failures,
/// required field boundary rules via CustomerProfileValidator.
/// </summary>
public class UpdateCustomerProfileValidationTests
{
    private static readonly Guid AnyCustomerId = Guid.NewGuid();

    // ─── CustomerProfileValidator – name rules ────────────────────────────────

    [Fact]
    public void Validator_EmptyFirstName_ReturnsFirstNameError()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "",
            lastName: "Baker",
            email: "jamie@example.com",
            gender: CustomerProfileGender.Female,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("firstName");
    }

    [Fact]
    public void Validator_FirstNameExceedsMaxLength_ReturnsFirstNameError()
    {
        var validator = new CustomerProfileValidator();
        var tooLong = new string('A', 101);

        var result = validator.Validate(
            firstName: tooLong,
            lastName: "Baker",
            email: "jamie@example.com",
            gender: null,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("firstName");
    }

    [Fact]
    public void Validator_EmptyLastName_ReturnsLastNameError()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: "  ",
            email: "jamie@example.com",
            gender: null,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("lastName");
    }

    [Fact]
    public void Validator_LastNameExceedsMaxLength_ReturnsLastNameError()
    {
        var validator = new CustomerProfileValidator();
        var tooLong = new string('B', 101);

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: tooLong,
            email: "jamie@example.com",
            gender: null,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("lastName");
    }

    // ─── CustomerProfileValidator – email format ──────────────────────────────

    [Fact]
    public void Validator_InvalidEmailFormat_ReturnsEmailError()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: "Baker",
            email: "not-an-email",
            gender: null,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("email");
        result.Errors["email"].Should().Contain(e => e.Contains("valid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validator_EmptyEmail_ReturnsEmailRequiredError()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: "Baker",
            email: "",
            gender: null,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("email");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("test@sub.domain.org")]
    public void Validator_ValidEmails_PassEmailValidation(string email)
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate("Jamie", "Baker", email, null, null);

        result.Errors.Should().NotContainKey("email");
    }

    // ─── CustomerProfileValidator – gender ───────────────────────────────────

    [Fact]
    public void Validator_UndefinedGenderValue_ReturnsGenderError()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: "Baker",
            email: "jamie@example.com",
            gender: (CustomerProfileGender)999,
            dateOfBirth: null);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("gender");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(CustomerProfileGender.Male)]
    [InlineData(CustomerProfileGender.Female)]
    [InlineData(CustomerProfileGender.PreferNotToDisclose)]
    public void Validator_SupportedGenderValues_PassGenderValidation(CustomerProfileGender? gender)
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate("Jamie", "Baker", "jamie@example.com", gender, null);

        result.Errors.Should().NotContainKey("gender");
    }

    // ─── CustomerProfileValidator – date of birth ─────────────────────────────

    [Fact]
    public void Validator_FutureDateOfBirth_ReturnsDateOfBirthError()
    {
        var validator = new CustomerProfileValidator();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: "Baker",
            email: "jamie@example.com",
            gender: null,
            dateOfBirth: futureDate);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("dateOfBirth");
    }

    [Fact]
    public void Validator_TodayAsDateOfBirth_ReturnsDateOfBirthError()
    {
        var validator = new CustomerProfileValidator();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = validator.Validate("Jamie", "Baker", "jamie@example.com", null, today);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("dateOfBirth");
    }

    [Fact]
    public void Validator_PastDateOfBirth_PassesDateOfBirthValidation()
    {
        var validator = new CustomerProfileValidator();
        var past = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-25);

        var result = validator.Validate("Jamie", "Baker", "jamie@example.com", null, past);

        result.Errors.Should().NotContainKey("dateOfBirth");
    }

    [Fact]
    public void Validator_NullDateOfBirth_PassesDateOfBirthValidation()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate("Jamie", "Baker", "jamie@example.com", null, null);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContainKey("dateOfBirth");
    }

    // ─── CustomerProfileValidator – multiple errors ───────────────────────────

    [Fact]
    public void Validator_MultipleInvalidFields_ReturnsAllErrors()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "",
            lastName: "",
            email: "bad-email",
            gender: null,
            dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("firstName");
        result.Errors.Should().ContainKey("lastName");
        result.Errors.Should().ContainKey("email");
        result.Errors.Should().ContainKey("dateOfBirth");
    }

    [Fact]
    public void Validator_AllValidFields_ReturnsNoErrors()
    {
        var validator = new CustomerProfileValidator();

        var result = validator.Validate(
            firstName: "Jamie",
            lastName: "Baker",
            email: "jamie@example.com",
            gender: CustomerProfileGender.Female,
            dateOfBirth: new DateOnly(1990, 5, 12));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ─── CustomerEmailUniquenessService – field-specific messages ────────────

    [Fact]
    public async Task EnsureEmailAvailable_VerifiedEmailConflict_ThrowsWithEmailFieldError()
    {
        var checker = Substitute.For<ICustomerEmailUniquenessChecker>();
        checker.CheckAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CustomerEmailUniquenessResult.Conflict(
                "duplicate@example.com",
                CustomerEmailConflictKind.VerifiedEmail));

        var service = new CustomerEmailUniquenessService(checker);

        Func<Task> act = () => service.EnsureEmailAvailableForPendingChangeAsync(
            "duplicate@example.com", AnyCustomerId, default);

        var ex = await act.Should().ThrowAsync<CustomerProfileValidationException>();
        ex.Which.FieldErrors.Should().ContainKey("email");
        ex.Which.FieldErrors["email"].Should().Contain(
            m => m.Contains("already", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task EnsureEmailAvailable_PendingEmailConflict_ThrowsWithEmailFieldError()
    {
        var checker = Substitute.For<ICustomerEmailUniquenessChecker>();
        checker.CheckAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CustomerEmailUniquenessResult.Conflict(
                "pending@example.com",
                CustomerEmailConflictKind.PendingEmail));

        var service = new CustomerEmailUniquenessService(checker);

        Func<Task> act = () => service.EnsureEmailAvailableForPendingChangeAsync(
            "pending@example.com", AnyCustomerId, default);

        var ex = await act.Should().ThrowAsync<CustomerProfileValidationException>();
        ex.Which.FieldErrors.Should().ContainKey("email");
    }

    [Fact]
    public async Task EnsureEmailAvailable_NoConflict_DoesNotThrow()
    {
        var checker = Substitute.For<ICustomerEmailUniquenessChecker>();
        checker.CheckAsync(
            Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CustomerEmailUniquenessResult.Available("available@example.com"));

        var service = new CustomerEmailUniquenessService(checker);

        Func<Task> act = () => service.EnsureEmailAvailableForPendingChangeAsync(
            "available@example.com", AnyCustomerId, default);

        await act.Should().NotThrowAsync();
    }
}
