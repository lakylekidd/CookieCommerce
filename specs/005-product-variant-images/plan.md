# Implementation Plan: Product Variant Images

**Branch**: `master` | **Date**: 2026-07-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-product-variant-images/spec.md`: allow creating product variants from the existing backoffice product page, upload one product-level main image, upload one or more variant photos, and update bootstrap data from existing static variant defaults.

## Summary

Implement product variants and product media as ProductDomain-owned behavior while preserving the existing Blazor backoffice product page as the user surface. ProductAggregate should own variants and media associations through event-sourced commands/events; image bytes should be stored in a configurable durable product-media store and linked by ProductMediaAsset references rather than embedded in product text fields or event history. Product read models/API responses and Web typed services must expose variants, product main image, and variant photos. Bootstrap should map existing static inventory/variant defaults (including image references) into ProductDomain seed data and fail clearly on missing images.

## Technical Context

**Language/Version**: .NET 10 / C# 14, Blazor Interactive Server components.

**Primary Dependencies**: Marten event store/projections, MediatR command/query handlers, ASP.NET Core ProductDomain API, Blazor Web, Aspire configuration/orchestration, existing Web `IBackofficeService`/ProductsArea components, xUnit/FluentAssertions/Moq for tests.

**Storage**: Product state and media metadata/associations in Marten/PostgreSQL events and projections; image binary content in a configurable ProductDomain media store (development filesystem/volume via Aspire, production-ready blob/object-store seam). Domain events store media references/metadata, not bytes.

**Testing**: xUnit, FluentAssertions, Moq. Add ProductDomain aggregate/application/projection/API integration tests for variants/media; add Web service/component tests for existing page workflow and validation preservation. Do not add tests for behavioral orchestration or bootstrapping projects; validate bootstrap behavior through quickstart/manual scenarios instead. Run `dotnet build src`, targeted ProductDomain/Web tests, and `dotnet test src` before merge for broad domain/API/UI behavior.

**Target Platform**: Aspire-orchestrated CookieCommerce services, ProductDomain API, Marten/PostgreSQL, and Blazor Web backoffice UI.

**Project Type**: ProductDomain bounded-context change plus Blazor Web UI/service integration and bootstrap data update.

**Performance Goals**: Backoffice product reload should return variants/media from read models without loading image bytes. Common valid product/variant image uploads should complete without forcing form re-entry and meet the spec's 95% success expectation for common valid uploads.

**Constraints**: Existing backoffice product page remains the surface; no separate variant-management page. Controllers stay thin; handlers are async and accept/pass cancellation tokens. Domain remains persistence/media-store ignorant. Uploaded media must be removable from current views, must not introduce intentional PII, and must not store bytes in event history. Product main image is at most one current image; variant photos stay scoped to their owning variant. Configuration must flow through Aspire/appsettings/environment, not hard-coded paths/secrets.

**Scale/Scope**: One bounded context (`ProductDomain`), existing `CookieCommerce.Web` backoffice products area, ProductDomain bootstrap/demo data, product detail/list read models, and product media storage configuration. No new unrelated bounded context or customer purchase behavior is required.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Domain boundaries**: PASS. Variants and product media are ProductDomain concerns; implementation belongs in existing `src/CookieCommerce.ProductDomain.{Api,Application,Domain,Infrastructure,Tests}` plus `src/CookieCommerce.Web` typed UI integration.
- **Backend standards**: PASS. API additions must use `*Request`/`*Response`, delegate to MediatR commands/queries, keep DTO mappings explicit, validate at boundaries/use-case entry, and isolate media-store persistence in Infrastructure.
- **Event sourcing + CQRS**: PASS. Product variants and media associations are modeled through ProductAggregate events and projected into product read models. Image bytes are outside event history.
- **Testing discipline**: PASS. Plan requires aggregate invariant tests, handler validation tests, Marten projection/API integration tests, and Web-focused tests for form behavior. Bootstrap behavior will be validated manually through quickstart scenarios because the constitution forbids tests for behavioral orchestration or bootstrapping projects.
- **Async integration**: PASS. No cross-domain synchronous calls or new cross-domain integration events are required. Existing product snapshot/outbox behavior should be extended only if existing consumers require variant/media fields.
- **Privacy + operations**: PASS. No intentional PII is added. Product media may contain accidental sensitive content and remains removable; invalid uploads return ProblemDetails/validation errors. Logging must avoid file content and secrets.
- **Aspire + containerization**: PASS with implementation requirement. ProductDomain API is an affected runtime surface; if it lacks a Dockerfile, implementation must add/verify one and keep media storage configuration/AppHost wiring container-safe.
- **Frontend standards**: PASS. Changes stay in ProductsArea or focused shared components, use typed services, preserve accessibility/responsive behavior, and avoid direct backend calls from Razor.
- **Documentation impact**: PASS with implementation requirement. If `src/CookieCommerce.Web/Services/**/*.cs` interfaces change, update `Documentation/backend-documentation.md` in the same change.

Any failed gate MUST be listed in Complexity Tracking with a justification, the simpler alternative rejected, and the migration or mitigation path.

## Project Structure

### Documentation (this feature)

```text
specs/005-product-variant-images/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── backoffice-products-ui.md
│   └── product-variant-media-api.md
└── tasks.md             # Created later by /speckit.tasks, not by this plan command
```

### Source Code (repository root)

```text
src/
├── CookieCommerce.ProductDomain.Api/
│   ├── Controllers/ProductController.cs
│   ├── Models/Products/*Request.cs
│   ├── Models/Products/*Response.cs
│   ├── Middleware/DomainExceptionMiddleware.cs
│   └── Dockerfile                         # add/verify for affected runtime
├── CookieCommerce.ProductDomain.Application/
│   └── Products/
│       ├── Commands/                      # add/update variant + media commands
│       ├── Handlers/                      # async MediatR handlers
│       ├── Queries/
│       └── ReadModels/                    # include variants/media summaries
├── CookieCommerce.ProductDomain.Domain/
│   ├── Aggregates/ProductAggregate.cs
│   ├── DomainEvents/                      # variant/media events
│   ├── Exceptions/                        # duplicate/invalid variant/media exceptions
│   └── ValueObjects/                      # product media/variant value concepts if useful
├── CookieCommerce.ProductDomain.Infrastructure/
│   ├── DependencyInjection/
│   ├── Projections/                       # product details/list variant/media projections
│   ├── Media/                             # IProductMediaStore implementation + options
│   └── Bootstrap/                         # static-default validation/seeding if ProductDomain-owned
├── CookieCommerce.ProductDomain.Tests/
│   ├── DomainTests/
│   ├── ApplicationTests/
│   ├── IntegrationTests/
│   └── ProjectionsTests/
├── CookieCommerce.Web/
│   ├── Components/Backoffice/ProductsArea/
│   ├── Models/Products/
│   └── Services/
├── CookieCommerce.Web.Tests/
├── CookieCommerce.AppHost/                # media-store config/volume/service wiring
└── CookieCommerce.DevBootstrap/           # update if this remains the bootstrap entry point
```

**Structure Decision**: Extend the existing ProductDomain five-project layout for behavior and persistence, use the existing Web ProductsArea for the backoffice surface, and update bootstrap where current development/demo seeding lives. Do not add a new bounded context or separate product/variant management UI.

## Phase 0: Research Summary

See [research.md](./research.md).

Resolved decisions:

- Store image content outside domain events and keep ProductMediaAsset references in ProductDomain.
- Use a configurable product-media store with local filesystem/volume for development and a blob-compatible seam for production.
- Validate images at the API/use-case boundary before domain mutation.
- Model variants as ProductAggregate-owned child entities changed through event-sourced commands.
- Keep Backoffice UI in existing ProductsArea and use typed service methods with multipart-capable requests.
- Bootstrap defaults come from existing static inventory/variant data and fail fast on missing image files.
- No new cross-domain integration events are required for the requested workflow.

## Phase 1: Design Summary

See [data-model.md](./data-model.md), [contracts/product-variant-media-api.md](./contracts/product-variant-media-api.md), [contracts/backoffice-products-ui.md](./contracts/backoffice-products-ui.md), and [quickstart.md](./quickstart.md).

Implementation should preserve these contracts:

1. ProductAggregate owns variants, product main image association, and variant photo associations.
2. Product media bytes are written to configured storage; ProductDomain events/read models store metadata and references only.
3. Product detail/list read models include product main image and variant/media summaries needed by Web.
4. Existing backoffice product page supports adding/reviewing variants and uploading/replacing/removing images without a new page.
5. Product main image has at most one current association; variant photos remain scoped to one variant.
6. Bootstrap maps existing static defaults into product variants and images and reports missing defaults clearly.

## Agent Context Update

No `.specify` agent-context update script exists in this repository (`.specify/scripts/powershell/` contains setup/check scripts only), so no agent context file was updated by this plan command.

## Post-Design Constitution Check

- **Domain boundaries**: PASS. Data model and contracts keep ownership in ProductDomain and UI integration in `CookieCommerce.Web`.
- **Backend standards**: PASS. Contracts require API DTO suffixes, explicit mapping, thin controllers, validation before domain mutation, and isolated Infrastructure media storage.
- **Event sourcing + CQRS**: PASS. Design uses ProductAggregate events and read-model projection updates; media bytes stay outside events.
- **Testing discipline**: PASS. Quickstart and design identify aggregate, handler, projection, API, and Web validation coverage. Bootstrap coverage is manual/quickstart-only to comply with the constitution's ban on tests for behavioral orchestration or bootstrapping projects.
- **Async integration**: PASS. No synchronous cross-domain business decisions are introduced; existing snapshot/outbox extension is conditional only if already used.
- **Privacy + operations**: PASS. Media is removable from current views, no new intentional PII is collected, validation failures are user-facing, and storage config is operationally explicit.
- **Aspire + containerization**: PASS with implementation requirement. Media storage and ProductDomain API container/AppHost wiring are explicitly included in source layout and validation.
- **Frontend standards**: PASS. UI contract uses existing ProductsArea, typed services, accessible upload controls, responsive behavior, and validation preservation.
- **Documentation impact**: PASS with implementation requirement. Web service interface changes must update `Documentation/backend-documentation.md`.

## Complexity Tracking

No constitution violations requiring justification.
