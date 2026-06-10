using CookieCommerce.Web.Models.Addresses;

namespace CookieCommerce.Web.Models.Customers;

public class CustomerDto
{
    // ── Identity ─────────────────────────────────────────────────────────────

    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }

    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age => DateOfBirth.HasValue
        ? (int)((DateTime.Today - DateOfBirth.Value).TotalDays / 365.25)
        : null;

    // ── Account state ────────────────────────────────────────────────────────

    public DateTime MemberSince { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsNewsletterSubscribed { get; set; }

    // ── Order activity ───────────────────────────────────────────────────────

    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }

    /// <summary>
    /// Mirrors the reward-point formula used on <see cref="CookieCommerce.Web.Models.Order.OrderListItemDto"/>:
    /// 10 points per euro spent.
    /// </summary>
    public int RewardPoints => (int)(TotalSpent * 10);

    // ── Address ──────────────────────────────────────────────────────────────

    /// <summary>
    /// The customer's default address. Null when no address has been saved yet.
    /// </summary>
    public AddressBookItemDto? PrimaryAddress { get; set; }
}
