using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookieCommerce.Web.Models;
using CookieCommerce.Web.Models.Account;
using CookieCommerce.Web.Models.Addresses;
using CookieCommerce.Web.Models.Order;
using CookieCommerce.Web.Models.Products;
using CookieCommerce.Web.Models.Reviews;
using CookieCommerce.Web.Services.Commerce;

namespace CookieCommerce.Web.Services;

public sealed class CustomerDomainAccountService(
    IHttpClientFactory httpClientFactory,
    IAuthService authService,
    AccountFeatures accountFeatures,
    ILogger<CustomerDomainAccountService> logger) : IAccountService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task AddToFavoritesAsync(Guid productId) => accountFeatures.AddToFavoritesAsync(productId);
    public Task RemoveFromFavoritesAsync(Guid productId) => accountFeatures.RemoveFromFavoritesAsync(productId);
    public Task<bool> IsFavoriteAsync(Guid productId) => accountFeatures.IsFavoriteAsync(productId);
    public Task<ICollection<StorefrontProductDto>> GetFavoriteProductsAsync() => accountFeatures.GetFavoriteProductsAsync();
    public Task<bool> HasBirthdateAsync() => accountFeatures.HasBirthdateAsync();
    public Task<UserProfileDto?> GetProfileAsync() => accountFeatures.GetProfileAsync();
    public Task<ResultDto> UpdateProfileAsync(UserProfileDto profile) => accountFeatures.UpdateProfileAsync(profile);
    public Task<ICollection<OrderListItemDto>> GetOrdersAsync(int page, int pageSize) => accountFeatures.GetOrdersAsync(page, pageSize);
    public Task<int> GetTotalOrdersCountAsync() => accountFeatures.GetTotalOrdersCountAsync();
    public Task<OrderDto?> GetOrderByIdAsync(Guid orderId) => accountFeatures.GetOrderByIdAsync(orderId);
    public Task<int> GetOrderCountInNeedOfAttentionAsync() => accountFeatures.GetOrderCountInNeedOfAttentionAsync();
    public Task<TrackingDto?> GetTrackingAsync(Guid orderId) => accountFeatures.GetTrackingAsync(orderId);
    public Task SubmitReviewAsync(Guid productId, ReviewSubmitRequest review) => accountFeatures.SubmitReviewAsync(productId, review);
    public Task<(bool, int)> HasAlreadyReviewedProductIdAsync(Guid productId) => accountFeatures.HasAlreadyReviewedProductIdAsync(productId);

    public async Task<IList<AddressBookItemDto>> GetAddressesAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return [];

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var response = await client.GetAsync("api/customer/me/addresses");

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
                return [];

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("CustomerDomain address read returned {Status} for customer {CustomerId}", (int)response.StatusCode, user.Id);
                return [];
            }

            var addresses = await response.Content.ReadFromJsonAsync<List<CustomerAddressApiResponse>>(JsonOptions);
            return addresses?.Select(MapToAddressBookItem).ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read CustomerDomain addresses for customer {CustomerId}", user.Id);
            return [];
        }
    }

    public async Task<ResultDto> AddAddressAsync(AddressBookItemDto address)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return ResultDto.Fail("Sign in to add an address.");

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var response = await client.PostAsJsonAsync("api/customer/me/addresses", new AddCustomerAddressApiRequest(
                address.Icon,
                address.Label,
                address.FirstName,
                address.LastName,
                address.Street,
                address.HouseNumber,
                address.HouseNumberAddition,
                address.City,
                address.PostalCode,
                address.Province,
                address.Country,
                address.Phone,
                address.IsDefault), JsonOptions);

            if (response.IsSuccessStatusCode)
                return ResultDto.Ok("Address added successfully.");

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                return await ReadFieldErrorsAsync(response, "Please check the highlighted address fields.");

            if (response.StatusCode == HttpStatusCode.Conflict)
                return ResultDto.Fail("An active address with this label already exists.");

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return ResultDto.Fail("Sign in to add an address.");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return ResultDto.Fail("Customer profile was not found.");

            logger.LogWarning("CustomerDomain add address returned {Status} for customer {CustomerId}", (int)response.StatusCode, user.Id);
            return ResultDto.Fail("We could not save this address right now.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add CustomerDomain address for customer {CustomerId}", user.Id);
            return ResultDto.Fail("We could not save this address right now.");
        }
    }

    public async Task<ResultDto> DeleteAddressAsync(Guid addressId)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return ResultDto.Fail("Sign in to delete an address.");

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var response = await client.DeleteAsync($"api/customer/me/addresses/{addressId}");

            if (response.IsSuccessStatusCode)
                return ResultDto.Ok("Address deleted successfully.");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return ResultDto.Fail("Address not found.");

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return ResultDto.Fail("Sign in to delete an address.");

            logger.LogWarning("CustomerDomain delete address returned {Status} for customer {CustomerId}", (int)response.StatusCode, user.Id);
            return ResultDto.Fail("We could not delete this address right now.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete CustomerDomain address {AddressId} for customer {CustomerId}", addressId, user.Id);
            return ResultDto.Fail("We could not delete this address right now.");
        }
    }

    public async Task<ResultDto> SetAsPrimaryAsync(Guid addressId)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return ResultDto.Fail("Sign in to change your default address.");
        try
        {
            using var response = await CreateAuthorizedClient(user.Id).PutAsync($"api/customer/me/addresses/{addressId}/default", null);
            return response.IsSuccessStatusCode ? ResultDto.Ok("Default address updated.") : ResultDto.Fail("Unable to change your default address.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to update default address");
            return ResultDto.Fail("Unable to change your default address.");
        }
    }

    public async Task<AddressBookItemDto?> GetPrimaryAddressAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return null;

        try
        {
            var client = CreateAuthorizedClient(user.Id);
            var response = await client.GetAsync("api/customer/me/addresses/default");

            if (response.StatusCode == HttpStatusCode.NoContent)
                return null;

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("CustomerDomain default address read returned {Status} for customer {CustomerId}", (int)response.StatusCode, user.Id);
                return null;
            }

            var address = await response.Content.ReadFromJsonAsync<CustomerAddressApiResponse>(JsonOptions);
            return address is null ? null : MapToAddressBookItem(address);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read CustomerDomain default address for customer {CustomerId}", user.Id);
            return null;
        }
    }

    private async Task<Models.Auth.UserDto?> GetCurrentUserAsync()
    {
        await authService.InitializeAsync();
        return authService.CurrentUser;
    }

    private HttpClient CreateAuthorizedClient(Guid customerId)
    {
        var client = httpClientFactory.CreateClient("customer-domain-api");
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Customer-Id", customerId.ToString());
        return client;
    }

    private static async Task<ResultDto> ReadFieldErrorsAsync(HttpResponseMessage response, string fallbackMessage)
    {
        var result = ResultDto.Fail(fallbackMessage);

        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in errors.EnumerateObject())
                {
                    if (field.Value.ValueKind == JsonValueKind.Array)
                    {
                        result.FieldErrors[field.Name] = field.Value
                            .EnumerateArray()
                            .Select(x => x.GetString() ?? string.Empty)
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToArray();
                    }
                }
            }
        }
        catch (JsonException)
        {
        }

        return result;
    }

    private static AddressBookItemDto MapToAddressBookItem(CustomerAddressApiResponse address) => new()
    {
        Id = address.Id,
        Index = address.Index,
        Icon = address.Icon ?? "home",
        Label = address.Label,
        FirstName = address.FirstName,
        LastName = address.LastName,
        Street = address.Street,
        HouseNumber = address.HouseNumber,
        HouseNumberAddition = address.HouseNumberAddition,
        City = address.City,
        PostalCode = address.PostalCode,
        Province = address.Province,
        Country = address.Country,
        Phone = address.Phone,
        IsDefault = address.IsDefault
    };
}
