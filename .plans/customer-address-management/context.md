# Context: customer-address-management

## Intent
- Move customer address book get/add/delete from `FakeAccountService` mock behavior into CustomerDomain-owned behavior.
- Customer profile and checkout should use real CustomerDomain address data for authenticated customer.

## Decisions
- Treat CustomerDomain as owner; address book is part of customer profile PII.
- Use existing event-sourced customer aggregate path (`CustomerAddressAdded`, `CustomerAddressRemoved`) but deepen it to cover full UI address fields and default-address rules.
- Add current-customer address endpoints/contracts instead of relying on caller-supplied customer ids for customer-facing flows.
- Update Web typed service/client behavior so address operations call CustomerDomain and refresh shared profile state.

## Constraints
- Constitution requires event-sourced state changes, CQRS/read models, encryption-at-rest for PII, thin controllers, async handlers with cancellation tokens.
- Domain remains persistence/infrastructure ignorant.
- Web components must access backend through typed services only.
- Changes to `src/CookieCommerce.Web/Services/**/*.cs` interfaces/behavior require `Documentation/backend-documentation.md` update.

## Open questions
- None blocking. Default address reassignment uses oldest remaining active address per spec assumption.

## Discarded options
- Keep fake address state and only wrap it: rejected because user explicitly wants CustomerDomain implementation and mock data leaks across flows.
- Add a separate Address bounded context: rejected because addresses are customer profile data and existing CustomerDomain already contains customer address concepts.
- Synchronous cross-domain address lookup from other domains: rejected by constitution; not needed for this feature.
