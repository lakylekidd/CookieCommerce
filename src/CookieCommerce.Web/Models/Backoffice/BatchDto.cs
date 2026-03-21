namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// This model represents a batch of cookie production. 
/// </summary>
public class BatchDto
{
    /// <summary>
    /// The unique identifier for the batch.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The unique identifier for the product being produced in this batch.
    /// </summary>
    public Guid ProductId { get; set; }
    /// <summary>
    /// The reference code for the batch, which can be used to track and 
    /// identify the batch in the production process. These can be printed as QR codes
    /// so that they can be scanned at various stages of the production process to 
    /// update the batch status and track its progress.
    /// </summary>
    public string BatchReference { get; set; } = string.Empty;
    /// <summary>
    /// The current status of the batch, which can be used to track its progress through the production process.
    /// </summary>
    public BatchStatus BatchStatus { get; set; } = BatchStatus.Scheduled;
    /// <summary>
    /// This is the date and time when the batch was created. This can be used to track how long the batch 
    /// has been in production and to identify any potential bottlenecks or delays in the production process.
    /// </summary>
    public DateTime CreatedOn { get; set; }
    /// <summary>
    /// This is the planned quantity of cookies to be produced in this batch.
    /// </summary>
    public int PlannedQuantity { get; set; }
    /// <summary>
    /// This is the actual quantity of cookies produced in this batch.
    /// </summary>
    public int ActualQuantity { get; set; }
    /// <summary>
    /// The quantity of defective cookies produced in this batch.
    /// </summary>
    public int DefectiveQuantity { get; set; }
    /// <summary>
    /// Any remarks or notes about the batch.
    /// </summary>
    public string? Remarks { get; set; }
    /// <summary>
    /// The batch runs associated with this batch.
    /// </summary>
    public ICollection<BatchRunDto> BatchRuns { get; set; } = [];
}
