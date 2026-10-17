using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CookieCommerce.Web.Models.Products;

namespace CookieCommerce.Web.Services.Products;

public sealed record CatalogVariant(Guid Id, string Name, string? Sku, decimal Price, string CurrencySymbol, bool IsActive, List<ProductImageDto> Photos);
public sealed record CatalogProduct(Guid Id, string Name, string? Description, decimal Price, bool IsActive, ProductImageDto? MainImage, List<CatalogVariant> Variants);

public sealed class ProductApiClient(IHttpClientFactory factory)
{
    private HttpClient Client => factory.CreateClient("product-domain-api");
    public async Task<List<CatalogProduct>> ListAsync(CancellationToken ct = default) =>
        await Client.GetFromJsonAsync<List<CatalogProduct>>("api/product", ct) ?? [];

    public async Task<CatalogProduct?> GetAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await Client.GetAsync($"api/product/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<CatalogProduct>(cancellationToken: ct);
    }

    public async Task<Guid> CreateAsync(CreateProductDto product)
    {
        if (product.Variants.Count == 0) throw new InvalidOperationException("Add at least one variant.");
        using var response = await Client.PostAsJsonAsync("api/product", new
        {
            Name = product.ProductName, Description = product.ProductName,
            Price = product.Variants.Min(v => v.Price),
            Variants = product.Variants.Select(v => new { v.Name, v.Price, v.CurrencySymbol, IsActive = true })
        });
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task RenameAsync(Guid id, string name, CancellationToken ct)
    {
        var product = await GetAsync(id, ct) ?? throw new KeyNotFoundException("Product not found.");
        using var response = await Client.PutAsJsonAsync($"api/product/{id}", new { Name = name.Trim(), product.Price }, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task SetStatusAsync(Guid id, bool active)
    {
        using var response = await Client.PutAsJsonAsync($"api/product/{id}/status", new { IsActive = active });
        await EnsureSuccessAsync(response);
    }

    public async Task SaveVariantsAsync(Guid id, IReadOnlyCollection<UpsertProductVariantDto> variants, CancellationToken ct = default)
    {
        using var response = await Client.PutAsJsonAsync($"api/product/{id}/variants", variants, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<ProductImageDto> UploadAsync(string path, ProductMediaUploadDto upload, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        var content = new StreamContent(upload.Content);
        content.Headers.ContentType = new MediaTypeHeaderValue(upload.ContentType);
        form.Add(content, "file", Path.GetFileName(upload.FileName));
        if (upload.AltText is not null) form.Add(new StringContent(upload.AltText), "altText");
        if (upload.SortOrder is not null) form.Add(new StringContent(upload.SortOrder.Value.ToString()), "sortOrder");
        using var response = await Client.PostAsync(path, form, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ProductImageDto>(cancellationToken: ct))!;
    }

    public async Task DeleteAsync(string path, CancellationToken ct)
    {
        using var response = await Client.DeleteAsync(path, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        if (response.IsSuccessStatusCode) return;
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(cancellationToken: ct);
        throw new InvalidOperationException(problem?.Detail ?? problem?.Title ?? "The product could not be saved.");
    }
}
