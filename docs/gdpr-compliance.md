# 🛡️ GDPR Compliance Strategy: Crypto‑Shredding for Event‑Sourced Data

This service uses event sourcing with Marten, which means customer data is stored as immutable events. Under GDPR, customers have the right to be forgotten, which requires that personal data becomes permanently irretrievable. Since events cannot be deleted or mutated without breaking the event stream, this service implements a GDPR‑compliant approach known as crypto‑shredding.

Crypto‑shredding allows us to preserve the integrity of the event log while ensuring that personal data is effectively erased.

# 🔐 What is Crypto‑Shredding?
Crypto‑shredding is a GDPR‑compliant technique where:

Personal data is encrypted before being stored in events or aggregate state.

Each customer receives a unique encryption key.

When a customer requests deletion, we delete the encryption key, not the events.

Without the key, all encrypted data becomes permanently unreadable.

This approach satisfies GDPR’s “right to erasure” while preserving the immutability of the event store.

# 🧱 What Data Is Encrypted?
Only fields that appear in the event stream are encrypted, because events are immutable and may exist indefinitely in backups.

Encrypted fields include:

Email

Full name

Addresses (label + full address)

These values are encrypted before being passed to the aggregate or included in events.

# 📄 What Data Is Not Encrypted?
Some customer attributes are stored only in read models, not in events. These can be safely deleted without crypto‑shredding:

Any optional demographic or profile fields.

These fields are removed entirely during GDPR deletion.

# 🧬 How It Works Internally
1. Key Generation
When a customer is created:

A new 256‑bit symmetric encryption key is generated.

The key is stored in a dedicated CustomerKeys table.

The key is used to encrypt all personal data before it enters the event stream.

2. Event Storage
Events contain only encrypted values.
The aggregate stores encrypted values as well.

3. Read Models
Read models decrypt data using the customer’s key.
If the key is missing (e.g., after deletion), the read model cannot be rebuilt with personal data.

4. GDPR Deletion
When a customer invokes the right to be forgotten:

The encryption key is deleted from the key store.

All read models for that customer are deleted.

A CustomerDeleted event is emitted (contains no personal data).

After this point:

Events still exist, but all encrypted fields are unreadable.

No plaintext personal data remains anywhere in the system.

Backups are safe because they do not contain the key.

# 🧠 Why This Approach?
Crypto‑shredding provides several advantages:

GDPR compliant: Personal data becomes irretrievable.

Event‑sourcing friendly: No rewriting or deleting events.

Operationally simple: Key deletion is fast and safe.

Backup‑safe: Old backups cannot reveal personal data.

Minimal domain impact: The aggregate remains deterministic and pure.

# 🧭 Summary
This service uses crypto‑shredding to reconcile GDPR requirements with event sourcing:

Personal data in events is encrypted.

Each customer has a unique encryption key.

Deleting the key permanently anonymizes the event history.

Non‑event data (DOB, gender, etc.) is stored only in read models and deleted directly.

This approach ensures strong privacy guarantees while preserving the integrity and auditability of the event‑sourced architecture.