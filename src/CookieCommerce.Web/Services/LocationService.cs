
using CookieCommerce.Web.Models.Addresses;

namespace CookieCommerce.Web.Services;

public class LocationService(HttpClient http) : ILocationService
{
    public async Task<DutchAddressResult?> LookupDutchAddressAsync(string postCode, string houseNumber)
    {
        return await LookupDutchAddressAsync(postCode, houseNumber, string.Empty);
    }

    public async Task<DutchAddressResult?> LookupDutchAddressAsync(string postCode, string houseNumber, string addition)
    {
        if (string.IsNullOrWhiteSpace(postCode) || string.IsNullOrWhiteSpace(houseNumber)) return null;

        var cleanPostcode = postCode.Replace(" ", "").ToUpper();

        try
        {
            // Step 1: Suggest API - find the address ID
            // We filter for 'type:adres' to ensure we don't get streets or cities only.
            var suggestUrl = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/suggest?q={cleanPostcode} {houseNumber}&fq=type:adres";
            var suggestResult = await http.GetFromJsonAsync<PdokResponse>(suggestUrl);

            var addressId = suggestResult?.Response?.Docs?.FirstOrDefault()?.Id;

            if (addressId != null)
            {
                // Step 2: Lookup API - get the actual details
                var lookupUrl = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/lookup?id={addressId}";
                var lookupResult = await http.GetFromJsonAsync<PdokResponse>(lookupUrl);

                var finalDoc = lookupResult?.Response?.Docs?.FirstOrDefault();
                if (finalDoc != null)
                {
                    return new DutchAddressResult
                    {
                        Street = finalDoc.Straatnaam,
                        City = finalDoc.Woonplaatsnaam,
                        PostCode = cleanPostcode,
                        HouseNumber = houseNumber
                    };
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Jungle Error looking up address: {ex.Message}");
            return null;
        }
    }
}
