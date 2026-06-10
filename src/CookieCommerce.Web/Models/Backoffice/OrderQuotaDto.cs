namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// The order quota DTO represents the daily order quota for a specific date, 
/// including the total number of orders, the number of orders ready to fulfill, 
/// and the calculated daily quota based on these values.
/// </summary>
public class OrderQuotaDto
{
    /// <summary>
    /// The date for which the order quota is calculated. This represents the 
    /// date the orders are expected to be fulfilled and delivered, 
    /// not the date they were placed.
    /// </summary>
    public DateTime QuotaDate { get; set; }
    /// <summary>
    /// This is the total orders for the given quota date that are 
    /// expected to be fulfilled and delivered
    /// </summary>
    public int TotalOrders { get; set; }
    /// <summary>
    /// This is the number of orders that are ready to be fulfilled for the given quota date 
    /// meaning that we have baked enough cookies to fulfill these orders and they are ready 
    /// to be packaged and shipped.
    /// </summary>
    public int OrdersReadyToFulfill { get; set; }
    /// <summary>
    /// The current quota for the day.
    /// </summary>
    public int CurrentQuota { get; set; }
    /// <summary>
    /// This is the daily quota completed for the given date quota date calculated 
    /// based on the total orders and the orders ready to fulfill.
    /// </summary>
    public double DailyQuotaPercentage { get; set; }
    /// <summary>
    /// This is the number of cookies that have been baked today for the given quota date.
    /// </summary>
    public int BakedToday { get; set; }
    /// <summary>
    /// This is the number of cookies that still need to be baked today for the given quota date.
    /// </summary>
    public int RemainingToBake { get; set; }
}
