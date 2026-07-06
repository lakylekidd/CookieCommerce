---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are REQUIRED for behavioral changes. Include the lowest useful level of tests and add integration/contract tests when persistence, projections, APIs, messaging, or cross-domain behavior changes.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Bounded context**: `src/CookieCommerce.{Domain}.Api/`, `.Application/`, `.Domain/`, `.Infrastructure/`, `.Tests/`
- **Blazor UI**: `src/CookieCommerce.Web/`
- **Shared platform**: `src/CookieCommerce.DomainBuildingBlocks/`, `src/CookieCommerce.CookieMassTransit/`, `src/CookieCommerce.ServiceDefaults/`, `src/CookieCommerce.AppHost/`
- **Documentation**: `README.md`, `CLAUDE.md`, `Documentation/backend-documentation.md` when service interfaces change
- Paths shown below are examples - adjust based on plan.md structure

<!--
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.

  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/

  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment

  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting and formatting tools

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Setup Marten schema/projection configuration in src/CookieCommerce.{Domain}.Infrastructure/
- [ ] T005 [P] Implement authentication/authorization integration where required
- [ ] T006 [P] Setup API routing, DTO mapping, and middleware structure in src/CookieCommerce.{Domain}.Api/
- [ ] T007 Create aggregate/value object/read model foundations that all stories depend on
- [ ] T008 Configure domain exception mapping, ProblemDetails, logging, and health/telemetry integration
- [ ] T009 Setup Aspire/appsettings/user-secrets/environment-variable configuration management
- [ ] T010 Add or update Dockerfile for each affected bounded-context runtime and register/verify it in src/CookieCommerce.AppHost

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 ⚠️

> **NOTE: Write required tests first when practical; ensure they fail for the intended reason before implementation.**

- [ ] T011 [P] [US1] Unit test for aggregate/domain invariant in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs
- [ ] T012 [P] [US1] Integration or contract test for API/projection/message behavior in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs

### Implementation for User Story 1

- [ ] T013 [P] [US1] Create/update aggregate or value object in src/CookieCommerce.{Domain}.Domain/[Feature]/[Name].cs
- [ ] T014 [P] [US1] Create/update domain event or read model in src/CookieCommerce.{Domain}.Domain/ or .Infrastructure/
- [ ] T015 [US1] Implement command/query handler in src/CookieCommerce.{Domain}.Application/[Feature]/[Name].cs (depends on T013, T014)
- [ ] T016 [US1] Implement API endpoint or Blazor UI feature in src/CookieCommerce.{Domain}.Api/ or src/CookieCommerce.Web/
- [ ] T017 [US1] Add validation and domain exception handling
- [ ] T018 [US1] Apply backend standards for thin controllers, focused async handlers with cancellation tokens, explicit DTO mapping, and isolated infrastructure concerns
- [ ] T019 [US1] Add logging, ProblemDetails/domain exception mapping, and PII handling for user story 1 operations
- [ ] T020 [US1] Apply frontend standards for Razor components, typed services, scoped styles/JS interop, accessibility, and responsive behavior if UI changes are included

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 ⚠️

- [ ] T021 [P] [US2] Unit test for aggregate/domain invariant in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs
- [ ] T022 [P] [US2] Integration or contract test for API/projection/message behavior in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs

### Implementation for User Story 2

- [ ] T023 [P] [US2] Create/update aggregate, event, projection, or read model in the owning CookieCommerce project
- [ ] T024 [US2] Implement command/query handler in src/CookieCommerce.{Domain}.Application/[Feature]/[Name].cs
- [ ] T025 [US2] Implement API endpoint or Blazor UI feature in src/CookieCommerce.{Domain}.Api/ or src/CookieCommerce.Web/
- [ ] T026 [US2] Integrate with User Story 1 components or local snapshots (if needed)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 ⚠️

- [ ] T027 [P] [US3] Unit test for aggregate/domain invariant in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs
- [ ] T028 [P] [US3] Integration or contract test for API/projection/message behavior in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs

### Implementation for User Story 3

- [ ] T029 [P] [US3] Create/update aggregate, event, projection, or read model in the owning CookieCommerce project
- [ ] T030 [US3] Implement command/query handler in src/CookieCommerce.{Domain}.Application/[Feature]/[Name].cs
- [ ] T031 [US3] Implement API endpoint or Blazor UI feature in src/CookieCommerce.{Domain}.Api/ or src/CookieCommerce.Web/

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in README.md, CLAUDE.md, Documentation/backend-documentation.md, or docs/ as applicable
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX [P] Additional unit/integration tests in src/CookieCommerce.{Domain}.Tests/ where coverage gaps remain
- [ ] TXXX Security hardening, including encrypted PII and erasure behavior checks
- [ ] TXXX Frontend accessibility and responsive behavior validation for UI changes
- [ ] TXXX Build affected Docker image(s) and verify AppHost registration for affected bounded-context runtimes
- [ ] TXXX Run quickstart.md validation when present
- [ ] TXXX Run `dotnet build src` and the smallest relevant `dotnet test` command; run `dotnet test src` for broad domain/persistence/messaging/API changes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Required tests MUST be written before or alongside implementation and SHOULD fail for the intended reason before implementation when test-first work is practical
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit test for aggregate/domain invariant in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs"
Task: "Integration or contract test for API/projection/message behavior in src/CookieCommerce.{Domain}.Tests/[Feature]/[Name]Tests.cs"

# Launch independent domain/read-model work for User Story 1 together:
Task: "Create/update aggregate or value object in src/CookieCommerce.{Domain}.Domain/[Feature]/[Name].cs"
Task: "Create/update domain event or read model in src/CookieCommerce.{Domain}.Domain/ or .Infrastructure/"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing when test-first work is practical
- Do not introduce synchronous runtime HTTP calls between bounded contexts
- Update Documentation/backend-documentation.md whenever Web service interfaces change
- Keep backend code layered: thin controllers, focused async handlers, persistence-ignorant domain code, explicit DTO mapping, and isolated infrastructure concerns
- Keep each bounded-context runtime containerized with a Dockerfile and registered/runnable through Aspire AppHost
- Keep Razor components small, accessible, responsive, and routed through typed Web services for backend access
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
