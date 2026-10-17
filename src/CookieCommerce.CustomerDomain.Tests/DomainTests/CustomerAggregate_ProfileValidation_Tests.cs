using CookieCommerce.CustomerDomain.Domain.Aggregates;
using CookieCommerce.CustomerDomain.Domain.Exceptions;
using CookieCommerce.CustomerDomain.Domain.ValueObjects;
using FluentAssertions;

namespace CookieCommerce.CustomerDomain.Tests.DomainTests;

/// <summary>
/// US2 – domain validation: required names, supported gender, past date of birth.
/// Domain validates encrypted values (names must not be blank ciphertext); plaintext
/// boundary validation belongs to the application-layer CustomerProfileValidator.
/// DateOfBirth domain rule is on CustomerDateOfBirth.Create value object.
/// </summary>
public class CustomerAggregate_ProfileValidation_Tests
{
    private static CustomerAggregate BuildCreatedAggregate(Guid? id = null)
    {
        var agg = new CustomerAggregate();
        agg.Create(id ?? Guid.NewGuid(), "enc-email", "enc-first", "enc-last", "enc-gender", null);
        return agg;
    }

    // ─── First name ───────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_EmptyFirstName_ThrowsCustomerProfileValidationException()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("", "enc-last", "enc-gender", null);

        act.Should().Throw<CustomerProfileValidationException>()
            .WithMessage("*First name*");
    }

    [Fact]
    public void UpdateProfile_WhitespaceFirstName_ThrowsCustomerProfileValidationException()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("   ", "enc-last", "enc-gender", null);

        act.Should().Throw<CustomerProfileValidationException>();
    }

    [Fact]
    public void UpdateProfile_ValidFirstName_DoesNotThrow()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("enc-first-valid", "enc-last", "enc-gender", null);

        act.Should().NotThrow();
    }

    // ─── Last name ────────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProfile_EmptyLastName_ThrowsCustomerProfileValidationException()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("enc-first", "", "enc-gender", null);

        act.Should().Throw<CustomerProfileValidationException>()
            .WithMessage("*Last name*");
    }

    [Fact]
    public void UpdateProfile_WhitespaceLastName_ThrowsCustomerProfileValidationException()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("enc-first", "  ", "enc-gender", null);

        act.Should().Throw<CustomerProfileValidationException>();
    }

    [Fact]
    public void UpdateProfile_ValidLastName_DoesNotThrow()
    {
        var agg = BuildCreatedAggregate();

        Action act = () => agg.UpdateProfile("enc-first", "enc-last-valid", "enc-gender", null);

        act.Should().NotThrow();
    }

    // ─── Gender (supported enum values via value object) ──────────────────────

    [Theory]
    [InlineData(CustomerProfileGender.Male)]
    [InlineData(CustomerProfileGender.Female)]
    [InlineData(CustomerProfileGender.PreferNotToDisclose)]
    public void CustomerProfileGender_AllSupportedValues_AreDefined(CustomerProfileGender gender)
    {
        var isDefined = Enum.IsDefined(typeof(CustomerProfileGender), gender);

        isDefined.Should().BeTrue(
            because: $"{gender} must be a valid supported gender value");
    }

    [Fact]
    public void CustomerProfileGender_ArbitraryInteger_IsNotDefined()
    {
        var unsupportedGender = (CustomerProfileGender)999;

        var isDefined = Enum.IsDefined(typeof(CustomerProfileGender), unsupportedGender);

        isDefined.Should().BeFalse();
    }

    // ─── Date of birth (CustomerDateOfBirth value object) ────────────────────

    [Fact]
    public void CustomerDateOfBirth_Create_FutureDate_ThrowsArgumentOutOfRangeException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var futureDate = today.AddDays(1);

        Action act = () => CustomerDateOfBirth.Create(futureDate, today);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value");
    }

    [Fact]
    public void CustomerDateOfBirth_Create_TodaysDate_ThrowsArgumentOutOfRangeException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        Action act = () => CustomerDateOfBirth.Create(today, today);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CustomerDateOfBirth_Create_PastDate_Succeeds()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pastDate = today.AddYears(-25);

        var result = CustomerDateOfBirth.Create(pastDate, today);

        result.Value.Should().Be(pastDate);
    }

    [Fact]
    public void CustomerDateOfBirth_Create_YesterdayDate_Succeeds()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var result = CustomerDateOfBirth.Create(yesterday, today);

        result.Value.Should().Be(yesterday);
    }

    // ─── Aggregate-level: no events saved on validation failure ──────────────

    [Fact]
    public void UpdateProfile_ValidationFailure_AppendsNoDomainEvents()
    {
        var agg = BuildCreatedAggregate();
        var eventsBefore = agg.DomainEvents.Count;

        Action act = () => agg.UpdateProfile("", "enc-last", "enc-gender", null);

        act.Should().Throw<CustomerProfileValidationException>();
        agg.DomainEvents.Should().HaveCount(eventsBefore,
            because: "validation failures must not append any events");
    }
}
