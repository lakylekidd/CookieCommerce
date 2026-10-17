using CookieCommerce.AuthenticationManagement.Application.Auth.Integration;
using FluentAssertions;

namespace CookieCommerce.AuthenticationManagement.Tests.ApplicationTests;

public sealed class PasswordSetupInvitationTests
{
    [Fact]
    public void HashToken_SameToken_IsDeterministicWithoutRetainingToken()
    {
        const string token = "a-sensitive-single-use-token";

        var first = PasswordSetupInvitation.HashToken(token);
        var second = PasswordSetupInvitation.HashToken(token);

        first.Should().Be(second).And.HaveLength(64);
        first.Should().NotContain(token);
    }
}
