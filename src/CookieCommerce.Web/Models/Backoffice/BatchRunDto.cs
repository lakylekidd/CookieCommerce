namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// This model represents a single run of a batch through the production process. 
/// A batch can have multiple runs. Each run will have its own status and timestamps 
/// for when it started and completed. This allows us to track the history of each batch 
/// and identify any potential issues in the production process.
/// </summary>
/// <remarks>
/// Batch runs are pre-created in bulk without a batch ID. This allows us to use these batch runs 
/// as placeholders so that we can print their reference codes as QR stickers and attach them to the batches. 
/// When a batch is created, we can then assign the batch runs to the batch by updating their batch ID. 
/// This way, we can track the progress of each batch through the production process using the batch runs, 
/// even if the batch runs were created before the batch itself was created.
/// </remarks>
public class BatchRunDto
{
    /// <summary>
    /// The unique identifier for the batch run.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The reference code for the batch run, which can be used to track and identify the batch run.
    /// </summary>
    public string BatchRunReference { get; set; } = string.Empty;
    /// <summary>
    /// The unique identifier for the batch that this batch run is associated with. 
    /// This can be null if the batch run has not yet been assigned to a batch yet.
    /// </summary>
    public Guid? BatchId { get; set; } = Guid.Empty;
    /// <summary>
    /// The current status of the batch run.
    /// </summary>
    public BatchRunStatus Status { get; set; } = BatchRunStatus.Scheduled;
    /// <summary>
    /// The date and time when the batch run started.
    /// </summary>
    public DateTime? StartedOn { get; set; }
    /// <summary>
    /// The date and time when the batch run completed.
    /// </summary>
    public DateTime? CompletedOn { get; set; }
    /// <summary>
    /// The baking information associated with this batch run.
    /// </summary>
    public BakingInformationDto? BakingInformation { get; set; }
}
