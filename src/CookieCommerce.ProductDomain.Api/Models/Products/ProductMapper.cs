using CookieCommerce.ProductDomain.Application.Products.Commands;

namespace CookieCommerce.ProductDomain.Api.Models.Products;

public static class ProductMapper
{
    public static CreateProduct ToCommand(this CreateProductRequest request) =>
        new CreateProduct(request.Name, request.Description, request.Price);

    public static RenameProduct ToRenameCommand(this UpdateProductRequest request, Guid productId) =>
        new RenameProduct(productId, request.Name);

    public static ChangeProductPrice ToChangePriceCommand(this UpdateProductRequest request, Guid productId) =>
        new ChangeProductPrice(productId, request.Price);
}
