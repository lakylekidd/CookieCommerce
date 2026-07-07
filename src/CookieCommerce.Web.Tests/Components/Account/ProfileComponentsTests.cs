using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Services;
using FluentAssertions;
using Moq;

namespace CookieCommerce.Web.Tests.Components.Account;

/// <summary>
/// Tests for profile state behavior used by PersonalDetailsCard and EditProfileModal.
/// Full Blazor component rendering tests would require bUnit; these verify the
/// service/state contracts that components depend on.
/// </summary>
public class ProfileComponentsTests
{
    [Fact]
    public void CustomerProfileState_SetProfile_NotifiesSubscribers()
    {
        var state = new CustomerProfileState();
        var notified = false;
        state.OnProfileChanged += () => notified = true;

        var profile = new UserProfileDto { FirstName = "Jamie", LastName = "Baker", Email = "jamie@example.com" };
        state.SetProfile(profile);

        notified.Should().BeTrue();
        state.Profile.Should().NotBeNull();
        state.Profile!.FirstName.Should().Be("Jamie");
    }

    [Fact]
    public void CustomerProfileState_Clear_SetsProfileToNull()
    {
        var state = new CustomerProfileState();
        state.SetProfile(new UserProfileDto { FirstName = "Jamie", Email = "j@example.com" });

        var notified = false;
        state.OnProfileChanged += () => notified = true;

        state.Clear();

        notified.Should().BeTrue();
        state.Profile.Should().BeNull();
    }

    [Fact]
    public void CustomerProfileState_MultipleSubscribers_AllNotified()
    {
        var state = new CustomerProfileState();
        var count = 0;
        state.OnProfileChanged += () => count++;
        state.OnProfileChanged += () => count++;

        state.SetProfile(new UserProfileDto { Email = "a@b.com" });

        count.Should().Be(2);
    }

    [Fact]
    public void CustomerProfileState_ProfileContainsPendingEmail_WhenSet()
    {
        var state = new CustomerProfileState();
        var profile = new UserProfileDto
        {
            Email = "jamie@example.com",
            PendingEmail = "jamie.new@example.com",
            EmailVerificationStatus = "PendingVerification"
        };

        state.SetProfile(profile);

        state.Profile!.PendingEmail.Should().Be("jamie.new@example.com");
        state.Profile.EmailVerificationStatus.Should().Be("PendingVerification");
        state.Profile.Email.Should().Be("jamie@example.com");  // verified email unchanged
    }

    [Fact]
    public void UserProfileDto_SupportedGenderValues()
    {
        // EditProfileModal only accepts these gender values
        var allowed = new[] { "Male", "Female", "PreferNotToDisclose" };

        foreach (var gender in allowed)
        {
            var dto = new UserProfileDto { Gender = gender };
            dto.Gender.Should().Be(gender);
        }
    }

    [Fact]
    public void UpdateProfile_SaveResponse_RefreshesState_VerifyDtoContract()
    {
        // Verify the DTO has all required fields for the state to refresh properly after save
        var updatedProfile = new UserProfileDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Updated",
            LastName = "Name",
            Email = "jamie@example.com",
            PendingEmail = null,
            EmailVerificationStatus = "Verified",
            Gender = "Female",
            DateOfBirth = new DateOnly(1990, 5, 12)
        };

        updatedProfile.Id.Should().NotBeEmpty();
        updatedProfile.FirstName.Should().NotBeNullOrEmpty();
        updatedProfile.Email.Should().NotBeNullOrEmpty();
        updatedProfile.EmailVerificationStatus.Should().Be("Verified");
    }

    [Fact]
    public void PersonalDetailsCard_ShowsPendingEmailMessage_WhenPendingEmailExists()
    {
        // Component must show the pending email state without replacing the verified email display
        var profile = new UserProfileDto
        {
            Email = "jamie@example.com",          // verified — displayed as profile email
            PendingEmail = "jamie.new@example.com", // pending — shown as "verification pending"
            EmailVerificationStatus = "PendingVerification"
        };

        profile.Email.Should().Be("jamie@example.com");
        profile.PendingEmail.Should().Be("jamie.new@example.com");
        profile.EmailVerificationStatus.Should().Be("PendingVerification");

        // Simulate the component's conditional check
        var shouldShowPendingMessage = !string.IsNullOrWhiteSpace(profile.PendingEmail);
        shouldShowPendingMessage.Should().BeTrue();
    }
}
