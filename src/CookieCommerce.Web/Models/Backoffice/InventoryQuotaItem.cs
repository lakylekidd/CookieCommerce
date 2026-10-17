namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// The status of an inventory quota item, which can be used to determine 
/// if the item is ready for fulfillment for the current quota date or not.
/// </summary>
public enum InventoryQuotaItemStatus
{
    /// <summary>
    /// The inventory quota item has not been started yet which means that 
    /// none of the required quantity has been received yet.
    /// </summary>
    NotStarted,
    /// <summary>
    /// The inventory quota item is in progress, which means that some but
    /// not all of the required quantity has been received yet.
    /// </summary>
    InProgress,
    /// <summary>
    /// The inventory quota item is ready for fulfillment, which means that the
    /// required quantity has been received.
    /// </summary>
    ReadyForFulfillment
}
