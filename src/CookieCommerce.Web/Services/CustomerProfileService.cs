using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;

namespace CookieCommerce.Web.Services;

/// <summary>
/// Concrete live CustomerDomain profile service using Aspire-named HttpClient.
/// Provides customer-facing profile load/save through the customer API.
/// No mock fallback — API failures surface as user-safe error states.
/// </summary>
public sealed class CustomerProfileService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;
    private readonly CustomerProfileState _profileState;
    private readonly ILogger<CustomerProfileService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CustomerProfileService(
        IHttpClientFactory httpClientFactory,
        IAuthService authService,
        CustomerProfileState profileState,
        ILogger<CustomerProfileService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _profileState = profileState;
        _logger = logger;
    }

    /// <summary>
    /// Loads the live profile from CustomerDomain GET /api/customer/me.
    /// Caches result in CustomerProfileState for immediate UI refresh.
    /// </summary>
    public async Task<UserProfileDto?> GetProfileAsync()
    {
        await _authService.InitializeAsync();
        var user = _authService.CurrentUser;
        if (user is null) return null;

        // Return cached profile if available
        if (_profileState.Profile is { } cached && cached.Id == user.Id) return cached;

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var response = await client.GetAsync("api/customer/me");

            if (response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.Forbidden)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "CustomerDomain GET /api/customer/me returned {Status} for customer {CustomerId}",
                    (int)response.StatusCode, user.Id);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<CustomerProfileApiResponse>(JsonOptions);
            if (dto is null) return null;

            var profile = MapToDto(dto, user);
            _profileState.SetProfile(profile);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load profile from CustomerDomain for customer {CustomerId} [failureType={FailureType}]", user.Id, ex.GetType().Name);
            return null;
        }
    }

    /// <summary>
    /// Updates the live profile via CustomerDomain PATCH /api/customer/me/profile.
    /// On success, immediately refreshes CustomerProfileState so all surfaces update.
    /// </summary>
    public async Task<ResultDto> UpdateProfileAsync(UserProfileDto profile)
    {
        await _authService.InitializeAsync();
        var user = _authService.CurrentUser;
        if (user is null) return ResultDto.Fail("Sign in to update your profile.");

        try
        {
            var client = CreateAuthorizedClient(user.Id);

            var request = new
            {
                firstName = profile.FirstName,
                lastName = profile.LastName,
                email = profile.Email,
                gender = MapGenderForApi(profile.Gender),
                dateOfBirth = profile.DateOfBirth?.ToString("yyyy-MM-dd")
            };

            var response = await client.PatchAsJsonAsync("api/customer/me/profile", request, JsonOptions);

            if (response.StatusCode == HttpStatusCode.Conflict)
                return new ResultDto
                {
                    Success = false,
                    Message = "Some fields are invalid. Please correct and try again.",
                    FieldErrors = new Dictionary<string, string[]>
                    {
                        ["email"] = ["Email address is already in use by another account."]
                    }
                };

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var errors = await TryReadValidationErrors(response);
                return new ResultDto
                {
                    Success = false,
                    Message = "Some fields are invalid. Please correct and try again.",
                    FieldErrors = errors
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "CustomerDomain PATCH /api/customer/me/profile returned {Status}",
                    (int)response.StatusCode);
                return ResultDto.Fail("Unable to save your profile at this time. Please try again shortly.");
            }

            var updated = await response.Content.ReadFromJsonAsync<CustomerProfileApiResponse>(JsonOptions);
            if (updated is not null)
            {
                var updatedProfile = MapToDto(updated, user);
                _profileState.SetProfile(updatedProfile);
                _authService.UpdateCurrentUserProfile(updatedProfile.FirstName, updatedProfile.LastName, updatedProfile.Email);
            }

            return ResultDto.Ok("Profile updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("UpdateProfileAsync failed for customer {CustomerId} [failureType={FailureType}]", user.Id, ex.GetType().Name);
            return ResultDto.Fail("Unable to save your profile at this time. Please try again shortly.");
        }
    }

    /// <summary>
    /// Requests or resends verification for the current pending email.
    /// </summary>
    public async Task<ResultDto> ResendEmailVerificationAsync(string? pendingEmail)
    {
        await _authService.InitializeAsync();
        var user = _authService.CurrentUser;
        if (user is null) return ResultDto.Fail("Sign in to resend verification.");

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var body = new { pendingEmail };
            var response = await client.PostAsJsonAsync("api/customer/me/email-verification", body, JsonOptions);

            if (!response.IsSuccessStatusCode)
                return ResultDto.Fail("Unable to send verification email at this time. Please try again shortly.");

            return ResultDto.Ok("Verification email sent. Continue signing in with your current email until verification is complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError("ResendEmailVerificationAsync failed for customer {CustomerId} [failureType={FailureType}]", user.Id, ex.GetType().Name);
            return ResultDto.Fail("Unable to send verification email at this time. Please try again shortly.");
        }
    }

    /// <summary>
    /// Confirms a pending email change and refreshes shared profile state from the success response.
    /// AuthenticationManagement may still need to consume the verified-email event before login changes.
    /// </summary>
    public async Task<ResultDto> ConfirmEmailVerificationAsync(string token)
    {
        await _authService.InitializeAsync();
        var user = _authService.CurrentUser;
        if (user is null) return ResultDto.Fail("Sign in to confirm your email change.");

        if (string.IsNullOrWhiteSpace(token))
            return ResultDto.Fail("The email verification link is invalid.");

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var response = await client.PostAsJsonAsync(
                "api/customer/me/email-verification/confirm",
                new { token },
                JsonOptions);

            if (response.StatusCode == HttpStatusCode.Gone)
                return ResultDto.Fail("This email verification link has expired. Request a new verification email from your profile.");

            if (!response.IsSuccessStatusCode)
                return ResultDto.Fail("We could not confirm this email verification link.");

            var updated = await response.Content.ReadFromJsonAsync<CustomerProfileApiResponse>(JsonOptions);
            if (updated is not null)
            {
                var updatedProfile = MapToDto(updated, user);
                _profileState.SetProfile(updatedProfile);
                _authService.UpdateCurrentUserProfile(updatedProfile.FirstName, updatedProfile.LastName, updatedProfile.Email);
            }

            return ResultDto.Ok("Email verified. Your profile email is updated; sign-in may change after account processing completes.");
        }
        catch (Exception ex)
        {
            _logger.LogError("ConfirmEmailVerificationAsync failed for customer {CustomerId} [failureType={FailureType}]", user.Id, ex.GetType().Name);
            return ResultDto.Fail("We could not confirm this email verification link.");
        }
    }

    /// <summary>Returns true if the customer has a date of birth on record.</summary>
    public async Task<bool> HasBirthdateAsync()
    {
        var profile = await GetProfileAsync();
        return profile?.DateOfBirth.HasValue == true;
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private HttpClient CreateAuthorizedClient(Guid customerId)
    {
        var client = _httpClientFactory.CreateClient("customer-domain-api");
        // Include customer ID in header; US3 (T071) will enforce JWT-based ownership
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Customer-Id", customerId.ToString());
        return client;
    }

    private static UserProfileDto MapToDto(CustomerProfileApiResponse api, Models.Auth.UserDto user)
    {
        return new UserProfileDto
        {
            Id = api.Id,
            FirstName = api.FirstName,
            LastName = api.LastName,
            Email = api.Email,
            PendingEmail = api.PendingEmail,
            EmailVerificationStatus = api.EmailVerificationStatus,
            Gender = api.Gender?.ToString(),
            DateOfBirth = api.DateOfBirth,
            Role = user.Role,
            Addresses = []
        };
    }

    private static int? MapGenderForApi(string? gender) => gender switch
    {
        null or "" => null,
        "Male" => 1,
        "Female" => 2,
        "PreferNotToDisclose" => 3,
        _ => null
    };

    private static async Task<Dictionary<string, string[]>> TryReadValidationErrors(HttpResponseMessage response)
    {
        try
        {
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                var result = new Dictionary<string, string[]>();
                foreach (var prop in errors.EnumerateObject())
                {
                    var messages = prop.Value.EnumerateArray()
                        .Select(v => v.GetString() ?? "")
                        .ToArray();
                    result[prop.Name] = messages;
                }
                return result;
            }
        }
        catch { /* swallow — return empty */ }

        return new Dictionary<string, string[]>();
    }

    // ─── API response shapes ──────────────────────────────────────────────────

    private sealed class CustomerProfileApiResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PendingEmail { get; set; }
        public string EmailVerificationStatus { get; set; } = "Verified";
        public GenderValue? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }

    private enum GenderValue
    {
        Male = 1,
        Female = 2,
        PreferNotToDisclose = 3
    }
}
