namespace CookieCommerce.OrderDomain.Application.Orders.ValueObjects;

public class PriceInfo
{
    public decimal BasePrice { get; set; }
    public TaxType TaxType { get; set; }
    public decimal TaxValue { get; set; }
    public bool IsTaxIncluded { get; set; }

    public decimal CalculateFinalPrice()
    {
        if (IsTaxIncluded)
            return BasePrice;

        return TaxType == TaxType.Percentage
            ? BasePrice + (BasePrice * TaxValue)
            : BasePrice + TaxValue;
    }
}

public enum TaxType
{
    Percentage = 0,
    Fixed = 1
}
