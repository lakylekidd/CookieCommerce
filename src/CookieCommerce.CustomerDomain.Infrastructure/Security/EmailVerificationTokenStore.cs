using System.Security.Cryptography;
using System.Text;
using CookieCommerce.CustomerDomain.Application.Customers.Integration;
using Marten;

namespace CookieCommerce.CustomerDomain.Infrastructure.Security;

/// <summary>
/// Marten-backed store for email verification token references.
/// Stores only the reference ID and a hashed form of the raw token.
/// Raw tokens are never persisted.
/// </summary>
public sealed class EmailVerificationTokenStore : IEmailVerificationTokenStore
{
    private readonly IDocumentSession _session;

    public EmailVerificationTokenStore(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<EmailVerificationTokenReference> CreateAsync(
        Guid customerId,
        string pendingEmail,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken)
    {
        // Generate a cryptographically random token
        var rawTokenBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToBase64String(rawTokenBytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('='); // URL-safe

        // Store only the hash — never the raw token
        var hashedToken = ComputeHash(rawToken);

        // Reference is a stable identifier for the token entry (not the raw token)
        var tokenReference = $"evt_{Guid.CreateVersion7():N}";

        var record = new EmailVerificationTokenRecord
        {
            Id = tokenReference,
            CustomerId = customerId,
            PendingEmailHint = MaskEmail(pendingEmail),  // no plain PII in storage
            HashedToken = hashedToken,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ConsumedAtUtc = null
        };

        _session.Store(record);
        await _session.SaveChangesAsync(cancellationToken);

        return new EmailVerificationTokenReference(
            customerId,
            tokenReference,
            hashedToken,
            expiresAtUtc,
            record.CreatedAtUtc,
            null);
    }

    public async Task<EmailVerificationTokenReference?> FindAsync(
        Guid customerId,
        string token,
        CancellationToken cancellationToken)
    {
        EmailVerificationTokenRecord? record = null;

        // The delivery contract may round-trip the opaque reference. Treat it as a
        // secret reference and accept it without ever logging or exposing it.
        if (token.StartsWith("evt_", StringComparison.Ordinal))
        {
            var byReference = await _session.LoadAsync<EmailVerificationTokenRecord>(token, cancellationToken);
            if (byReference?.CustomerId == customerId && byReference.ConsumedAtUtc is null)
                record = byReference;
        }

        if (record is null)
        {
            var hashedToken = ComputeHash(token);

            // Find by hashed raw token for the given customer when a raw token is used.
            record = await _session
                .Query<EmailVerificationTokenRecord>()
                .Where(r => r.CustomerId == customerId && r.HashedToken == hashedToken && r.ConsumedAtUtc == null)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (record is null)
            return null;

        return new EmailVerificationTokenReference(
            record.CustomerId,
            record.Id,
            record.HashedToken,
            record.ExpiresAtUtc,
            record.CreatedAtUtc,
            record.ConsumedAtUtc);
    }

    public async Task MarkConsumedAsync(
        Guid customerId,
        string tokenReference,
        DateTimeOffset consumedAtUtc,
        CancellationToken cancellationToken)
    {
        var record = await _session.LoadAsync<EmailVerificationTokenRecord>(tokenReference, cancellationToken);
        if (record is null || record.CustomerId != customerId) return;

        record.ConsumedAtUtc = consumedAtUtc;
        _session.Store(record);
        await _session.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0) return "***";
        var local = email[..atIndex];
        var masked = local.Length > 2
            ? $"{local[0]}***{local[^1]}{email[atIndex..]}"
            : $"***{email[atIndex..]}";
        return masked;
    }
}

/// <summary>
/// Marten document for persisting token references (no raw tokens stored).
/// </summary>
public sealed class EmailVerificationTokenRecord
{
    public string Id { get; set; } = default!;  // tokenReference
    public Guid CustomerId { get; set; }
    public string PendingEmailHint { get; set; } = string.Empty;  // masked
    public string HashedToken { get; set; } = default!;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
}
