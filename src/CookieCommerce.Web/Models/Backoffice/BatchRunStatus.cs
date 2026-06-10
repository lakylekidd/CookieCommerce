namespace CookieCommerce.Web.Models.Backoffice;

public enum BatchRunStatus
{
    /// <summary>
    /// It has been created but has not yet been assigned to a batch yet
    /// </summary>
    PendingAssignment,
    /// <summary>
    /// It has been scheduled for a batch but has not yet started.
    /// </summary>
    Scheduled,
    /// <summary>
    /// It is currently being baked in the oven.
    /// </summary>
    Baking,
    /// <summary>
    /// It is currently cooling down after being baked in the oven.
    /// </summary>
    Cooling,
    /// <summary>
    /// It is completed and ready for packaging and delivery.
    /// </summary>
    Completed,
    /// <summary>
    /// It has been cancelled and will not be processed further.
    /// </summary>
    Cancelled
}
