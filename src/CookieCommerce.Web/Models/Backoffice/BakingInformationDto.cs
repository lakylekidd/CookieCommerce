namespace CookieCommerce.Web.Models.Backoffice;

/// <summary>
/// This model represents the baking information for a batch run.
/// </summary>
public class BakingInformationDto
{
    /// <summary>
    /// The duration of the baking process in minutes.
    /// </summary>
    public int BakingDurationInMinutes { get; set; }
    /// <summary>
    /// The temperature at which the cookies were baked in degrees Celsius.
    /// </summary>
    public int TemperatureInCelsius { get; set; }
    /// <summary>
    /// Contains any additional settings or information about the oven used for baking.
    /// </summary>
    public string OvenSettings { get; set; } = string.Empty;
}
