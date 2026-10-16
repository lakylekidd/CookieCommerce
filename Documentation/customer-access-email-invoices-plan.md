# Customer access, outbound email, templates and PDF invoices

Implementation plan prepared 14 October 2026; implementation and development verification completed 15 October 2026. The original assessment below records the starting point and intended scope. See [the implementation and setup guide](customer-access-email-invoices.md) for delivered behavior, verification and configuration requirements. Production customer credentials have not been verified.

Implementation requirement added: every development mock customer, including `admin@tiger.com`, uses the default password `123`. Development startup restores that password and clears mock-account lockouts. This applies only to the seeded development accounts; public registration retains its password validation.

## Existing foundations

- AuthenticationManagement registers and authenticates customers in Keycloak. Registration publishes `UserRegisteredIntegrationEvent`, and CustomerDomain creates profiles with the Keycloak identifier as the customer ID.
- The authentication read model has a separate application user ID. Web sign-in normally uses the JWT subject, with the application ID as a fallback. Audit this contract across sign-in, refresh, profile access, carts and order ownership before changing it.
- `AuthController` already offers `forgot-password`, and `KeycloakAdminService` invokes `execute-actions-email` with `UPDATE_PASSWORD` and a 30-minute lifespan. Keycloak hosts the password-change form. The website's old reset-token route only points customers back to recovery.
- `CustomerDetail.razor` has customer details and order history but no account administration. `CustomerDto.IsActive` defaults to true and is not populated from Keycloak.
- Web already sends contact and operational mail using configuration-based SMTP. `CommerceAutomation` has a durable `CommerceEmail` queue with retries. Extend this foundation rather than starting another unrelated sender.
- Settings persists commerce preferences, but offers no SMTP editor or template manager. Sensitive typed configuration should not go into its reflection-based `AdditionalSettings` collection.
- CustomerDomain has an email-verification delivery adapter. With no delivery endpoint, it logs a handoff and skips sending; the external payload also needs a clear recipient-resolution contract.
- `OrderInvoice.razor` is an owner-scoped printable receipt. It does not generate or retain an issued PDF invoice.

## 1. Make customer access consistent

Audit all customer records against their linked Keycloak identities and authentication records. Report missing identities, duplicate or ambiguous matches, mismatched IDs, disabled users, required actions and failed registration synchronization. Do not equate successful authentication with successful access to the customer's existing profile and orders.

Keep existing customer IDs and order ownership stable. Add an explicit identity link where the IDs differ instead of rewriting historical order IDs. Use the link consistently across login, token refresh, account operations and administration. Normalize login-email whitespace and casing consistently without joining accounts on email alone.

Make registration synchronization durable and idempotent, with retries and an operational reconciliation path. Existing Keycloak users without a customer projection should be repaired without changing passwords. Missing login identities for eligible existing customer records should receive a first-password setup invitation after a verified linking process. Ambiguous matches require review. Forgotten or intentionally disabled accounts retain that state. Guest checkout remains supported; claiming guest orders requires proof of email ownership.

Verification: exercise all ten development accounts with their documented credentials, fresh registration, signup during checkout, an existing customer without credentials, profile loading, cart conversion, token refresh, email change and order ownership. Production passwords cannot be inspected; verify identity consistency and complete recovery flows with test accounts.

## 2. Add outbound email configuration and delivery

Add **Settings > Email** with SMTP host, port, explicit TLS mode, authentication username and password, sender name/address, reply-to, enabled state, and a test recipient. Store secrets encrypted, return only a configured/masked state to the browser, and define preserve/replace/clear behavior for saved credentials. Import existing environment configuration as the initial fallback.

Extract a common outbound-mail service from the existing contact and operational senders. Extend the durable queue with template version, message purpose, attachment references, retry count, expiry and useful delivery state. Use transactional enqueueing for commerce events, bounded retries and worker concurrency control. SMTP acceptance is recorded as sent; inbox delivery cannot be guaranteed. Idempotency limits duplicates, but SMTP does not provide exactly-once delivery.

Use the same settings for Keycloak authentication mail, with an explicit synchronization state and retry on failure. Verify the public Keycloak hostname, customer-facing HTTPS links and allowed return-to-sign-in redirect. A failed SMTP configuration must be visible to administrators.

Connect CustomerDomain's verification handoff to the sender with an authenticated service contract and an explicit recipient. Do not log credentials, reset links, verification links or raw tokens. Expired recovery messages must not continue retrying.

## 3. Build basic template management

Add **Settings > Templates** with a list of seeded templates, an editor, allowed-placeholder help, sample-data preview, save draft, publish and restore default. Email templates expose subject, HTML body and plain-text body. Seed password reset, first-password invitation, email verification, order confirmation and invoice email.

Support a small fixed placeholder vocabulary per template: for example customer name, store name, action URL, expiry and order reference. Escape substituted values, reject unsupported placeholders, require the action link on recovery messages, and disallow executable template code. Keep published versions so edits cannot change messages already queued. Start with one language and a simple editor; a drag-and-drop designer is outside the first release.

Password-reset customization requires a Keycloak integration: its action-email API sends the email itself and does not return a reset URL for the application to insert. Prototype a narrow Keycloak email-template provider against the deployed version. It obtains published template content through an authenticated internal endpoint, maps Keycloak's generated action link into the allowed placeholders and uses Keycloak's mail sender with synchronized SMTP settings. Keep a known-good default template when the template service is unavailable, and propagate SMTP failures. Pin and test the provider with Keycloak upgrades. Keep password/token validation in Keycloak.

The manager shares metadata, versions, placeholders and previews between email and invoice templates. PDF templates have a separate renderer and a limited invoice layout editor; email HTML is not reused as an invoice layout.

## 4. Add customer account administration and complete recovery

Add an **Account access** card to the back-office customer detail view. Show active, locked after failed attempts, disabled, password setup required, missing login identity or temporarily unavailable. Only display failure counts and unlock timing where Keycloak supplies reliable values.

Provide **Unblock account** and **Send password reset email** actions. Unblock clears that customer's brute-force failure state, then refreshes the status. It must not silently enable an intentionally disabled account. For a missing identity, show the provisioning/setup action defined in phase 1. Send recovery mail to the identity's current verified email, not an arbitrary administrator-entered address.

Implement protected backend commands and Keycloak adapters for status and per-user failure clearing. AuthenticationManagement currently has no explicit JWT authentication setup in its Program entry point; add verified authentication and server-side administrator authorization for these commands. UI visibility alone is insufficient. Use a least-privilege service account for Keycloak administration and record administrator, customer, action, time and outcome in an audit trail.

Keep self-service recovery's public response generic for known and unknown addresses. Add per-account and per-client rate limits, validation and private operational error reporting. Test valid, expired and reused links, required-action accounts, return-to-sign-in behavior, old-password rejection and new-password login. Establish a deliberate policy for existing sessions after reset and verify actual Keycloak behavior. Configure and test brute-force protection without resetting existing realm settings on startup.

## 5. Generate and retain PDF invoices

Add an invoice template alongside email templates, starting with one branded layout. Allow company details, logo, introductory/footer text and limited styling; keep line items, numbers and totals controlled by the renderer.

Introduce an invoice record with order ID, invoice number, issue date, template version, immutable financial/address/company snapshot, PDF storage reference and content hash. Persist currency, tax rates and tax amounts when the order is placed; current prices or VAT settings must not change an old invoice. Determine what reliable tax information is available for historical orders and flag incomplete records rather than inventing it.

Generate an issued invoice once verified payment confirmation is recorded. Use a unique per-order issuance key and a transactional number sequence so duplicate webhooks cannot issue another invoice. Failed PDF generation retries against the same invoice number and snapshot. Unpaid orders can have a clearly marked draft receipt; cancelled unpaid orders do not receive an issued invoice. Existing paid orders use an explicit, repeatable backfill job. Refund corrections are a later extension, and issued invoices are not overwritten.

Store generated PDFs privately and serve them through authorized downloads: order owners in their account and administrators in the back office. Guest access, if offered, uses a scoped expiring link. Add invoice download and send actions to order details. Optionally queue the invoice email with the stored PDF attached after successful generation.

Choose and verify the PDF library's deployment support and licensing during implementation. Verify A4 output, repeated table headers, long orders, long addresses, Unicode, discounts, shipping, rounding and tax totals. Build an explicit restore/retention policy for documents alongside database and encryption-key backups.

## 6. Validate and release

Run focused authentication, authorization, template, queue and invoice tests, then the affected existing domain and Web suites. Include integration coverage with Keycloak, PostgreSQL and a local SMTP capture server. Test restart/retry behavior, duplicate events, expired recovery mail, concurrent unlock requests and unauthorized access to customer actions or PDFs.

Perform browser checks for every development customer, recovery, the new account-access card, settings, template previews and invoice downloads. Use controlled test recipients before enabling real outbound mail. Reconcile identity links before enabling invitations; introduce template/email settings before account actions; enable invoice issuance only after snapshot and numbering migrations are ready.

Acceptance: eligible customer records have a working linked login or a verified first-password setup path; administrators can see and clear failed-login lockouts and initiate recovery; customer recovery completes through a real email; settings and template edits affect the appropriate senders; paid orders have stable downloadable PDFs and optional invoice emails.

Implementation order: identity audit and repair, email delivery, templates and the Keycloak bridge, account administration/recovery, then PDF invoices and backfill. SMTP credentials, the public hostname, company identity details and the treatment of historical invoices are deployment inputs. No feature implementation or customer account changes are included in this planning change.

## References

- [Existing commerce setup and service audit](commerce-services.md)
- [Keycloak Admin REST API: per-user brute-force state and action emails](https://www.keycloak.org/docs-api/latest/rest-api/index.html)
- [Keycloak EmailTemplateProvider interface](https://www.keycloak.org/docs-api/latest/javadocs/org/keycloak/email/EmailTemplateProvider.html)
- [Keycloak provider and theme development](https://www.keycloak.org/docs/latest/server_development/index.html)
