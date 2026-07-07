# Planning Context: Async customer deletion integration

## Intent

- Update the existing profile/deletion architecture plan so customer deletion does not let CustomerDomain make AuthenticationManagement login-identity business decisions directly.
- CustomerDomain should publish an async forgotten/deleted customer integration event through the outbox.
- AuthenticationManagement should consume that event idempotently and own login identity removal/disablement and email reuse behavior.

## Decisions

- Treat `CustomerForgotten` as the preferred event name for GDPR erasure language, with `CustomerDeleted` allowed if repository naming already uses delete terminology.
- Deletion records/events in CustomerDomain remain PII-free and crypto-shredding stays CustomerDomain-owned.
- AuthenticationManagement, not CustomerDomain, removes/disables login identities and releases login-email uniqueness after consuming the integration event.
- Login not-found and same-email re-registration are eventual-consistency outcomes after AuthenticationManagement processing.

## Constraints

- Constitution requires async cross-domain integration and forbids synchronous runtime business decisions across bounded contexts.
- CustomerDomain must remain persistence/infrastructure ignorant; integration event publication happens via application/outbox/infrastructure, not domain dependencies.
- Events/logs must avoid plaintext PII.
- Existing profile plan artifacts already exist and must be revised in place.

## Open questions

- None. Use `CustomerForgotten`/`CustomerDeleted` naming to match existing repo conventions during implementation.

## Discarded options

- CustomerDomain directly disables/deletes AuthenticationManagement identity: rejected; violates bounded context ownership and user instruction.
- CustomerDomain directly releases AuthenticationManagement email uniqueness: rejected; this is login-identity policy owned by AuthenticationManagement.
- Synchronous CustomerDomain-to-AuthenticationManagement API call during deletion: rejected by constitution async integration rule.
