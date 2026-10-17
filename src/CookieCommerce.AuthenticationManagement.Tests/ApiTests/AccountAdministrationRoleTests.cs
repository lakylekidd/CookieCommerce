using System.Security.Claims;
using CookieCommerce.AuthenticationManagement.Api.Controllers;
using CookieCommerce.AuthenticationManagement.Application.Keycloak;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CookieCommerce.AuthenticationManagement.Tests.ApiTests;

public sealed class AccountAdministrationRoleTests
{
    [Fact]
    public async Task DowngradeToCustomer_CurrentAdministrator_ReturnsConflict()
    {
        var identityId = Guid.NewGuid().ToString();
        var identities = new Mock<IKeycloakAdminService>();
        identities.Setup(x => x.GetUserByCustomerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeycloakUserInfo { Id = identityId });
        var controller = CreateController(identities, identityId);

        var result = await controller.DowngradeToCustomer(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
        identities.Verify(x => x.CountUsersInRealmRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        identities.Verify(x => x.RemoveRealmRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DowngradeToCustomer_LastAdministrator_ReturnsConflict()
    {
        var identities = new Mock<IKeycloakAdminService>();
        identities.Setup(x => x.GetUserByCustomerIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeycloakUserInfo { Id = "target" });
        identities.Setup(x => x.GetRealmRolesAsync("target", It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Admin"]);
        identities.Setup(x => x.CountUsersInRealmRoleAsync("Admin", It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var controller = CreateController(identities, "another-admin");

        var result = await controller.DowngradeToCustomer(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
        identities.Verify(x => x.RemoveRealmRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static AccountAdministrationController CreateController(Mock<IKeycloakAdminService> identities, string actorId)
    {
        var controller = new AccountAdministrationController(
            identities.Object,
            Mock.Of<IDocumentSession>(),
            Mock.Of<IHttpClientFactory>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", actorId)], "test"))
            }
        };
        return controller;
    }
}
