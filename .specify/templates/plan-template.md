# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]

**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [.NET 10 / C# 14, Blazor, or NEEDS CLARIFICATION]

**Primary Dependencies**: [Marten, MediatR, MassTransit, Aspire, Blazor, or NEEDS CLARIFICATION]

**Storage**: [Marten/PostgreSQL event store + read models, files, or N/A]

**Testing**: [xUnit, FluentAssertions, Testcontainers, WebApplicationFactory, or NEEDS CLARIFICATION]

**Target Platform**: [Aspire-orchestrated web/services, Blazor UI, or NEEDS CLARIFICATION]

**Project Type**: [bounded-context API/application/domain/infrastructure, Blazor web, shared library, or NEEDS CLARIFICATION]

**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]

**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]

**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Domain boundaries**: Does the feature belong to a named bounded context, and are API/Application/Domain/Infrastructure/Test responsibilities placed in the established `src/CookieCommerce.{Domain}.*` layout?
- **Backend standards**: Are controllers thin, handlers focused and async with cancellation tokens, DTOs mapped explicitly, domain code persistence-ignorant, and infrastructure concerns isolated?
- **Event sourcing + CQRS**: Are state changes modeled through aggregate events and command handlers, with affected read models/projections identified?
- **Testing discipline**: Are aggregate invariants, projections, API contracts, message consumers, or persistence behavior covered by unit/integration/contract tests as applicable?
- **Async integration**: Does cross-domain communication use integration events, the transactional outbox, and local snapshots/read models instead of synchronous runtime HTTP calls?
- **Privacy + operations**: Is sensitive PII encrypted before event storage, is erasure/key-shredding behavior preserved, and are logging/health/ProblemDetails behaviors accounted for?
- **Aspire + containerization**: Does each affected bounded-context runtime have a Dockerfile, avoid baked-in secrets, and remain registered/runnable through `src/CookieCommerce.AppHost`?
- **Frontend standards**: For `src/CookieCommerce.Web` changes, are components placed in the correct folder, API access routed through typed services, scoped styles/JS interop used appropriately, and accessibility/responsive behavior covered?
- **Documentation impact**: If `src/CookieCommerce.Web/Services/**/*.cs` interfaces change, does the plan include an update to `Documentation/backend-documentation.md`?

Any failed gate MUST be listed in Complexity Tracking with a justification, the simpler alternative rejected, and the migration or mitigation path.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# Bounded context change
src/
├── CookieCommerce.{Domain}.Api/        # includes Dockerfile for runtime service
├── CookieCommerce.{Domain}.Application/
├── CookieCommerce.{Domain}.Domain/
├── CookieCommerce.{Domain}.Infrastructure/
└── CookieCommerce.{Domain}.Tests/

# Shared platform or UI change
src/
├── CookieCommerce.DomainBuildingBlocks/
├── CookieCommerce.CookieMassTransit/
├── CookieCommerce.ServiceDefaults/
├── CookieCommerce.AppHost/
└── CookieCommerce.Web/                 # includes Dockerfile for Blazor runtime
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
