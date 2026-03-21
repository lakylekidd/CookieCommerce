namespace CookieCommerce.Web.Models.Backoffice;

public enum BatchStatus
{
    /// <summary>
    /// It has been created but has not yet been assigned to a batch run yet.
    /// </summary>
    PendingAssignment,
    /// <summary>
    /// It has been scheduled for production but has not yet started.
    /// </summary>
    Scheduled,
    /// <summary>
    /// It is currently being mixed.
    /// </summary>
    Mixing,
    /// <summary>
    /// It is currently resting after being mixed.
    /// </summary>
    Resting,
    /// <summary>
    /// Baking is completed and is ready for packaging and delivery.
    /// </summary>
    Completed,
    /// <summary>
    /// It has been cancelled and will not be processed further.
    /// </summary>
    Cancelled
}
