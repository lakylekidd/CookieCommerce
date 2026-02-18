using CookieCommerce.OrderDomain.Application.Customers;
using FluentAssertions;

namespace CookieCommerce.OrderDomain.Tests.ApplicationTests;

public class CustomerHelpersTests
{
    [Theory]
    [InlineData("john.doe@example.com", "j***@example.com")]
    [InlineData("j@example.com", "j***@example.com")]
    [InlineData("a@test.org", "a***@test.org")]
    [InlineData("longusername@domain.com", "l***@domain.com")]
    public void MaskEmail_ValidEmail_MasksLocalPart(string email, string expected)
    {
        // Act
        var result = CustomerHelpers.MaskEmail(email);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData(null, "")]
    public void MaskEmail_EmptyOrNull_ReturnsEmpty(string? email, string expected)
    {
        // Act
        var result = CustomerHelpers.MaskEmail(email!);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid-email", "***")]
    [InlineData("no-at-sign", "***")]
    public void MaskEmail_InvalidFormat_ReturnsAsterisks(string email, string expected)
    {
        // Act
        var result = CustomerHelpers.MaskEmail(email);

        // Assert
        result.Should().Be(expected);
    }
}
