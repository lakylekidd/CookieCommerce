# Tasks: Product Variant Images

**Input**: Design documents from `/specs/005-product-variant-images/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md, `.specify/memory/constitution.md`

**Tests**: Tests are REQUIRED by the repository constitution and this feature plan for aggregate invariants, handlers, projections, API contracts, and Web behavior. Do not add tests for behavioral orchestration or bootstrapping projects. Write tests before or alongside implementation when practical and verify they fail for the intended reason before implementing.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and has no dependency on incomplete tasks in the same phase.
- **[Story]**: User story label, required only for user story phases.
- Every task includes concrete repository file paths.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare product media storage, configuration, and runtime structure used by all stories.

- [X] T001 Create product media infrastructure folders in src/CookieCommerce.ProductDomain.Infrastructure/Media/ and src/CookieCommerce.ProductDomain.Infrastructure/DependencyInjection/
- [X] T002 [P] Add ProductDomain media configuration defaults in src/CookieCommerce.ProductDomain.Api/appsettings.json and src/CookieCommerce.ProductDomain.Api/appsettings.Development.json
- [X] T003 [P] Add or verify ProductDomain API container support in src/CookieCommerce.ProductDomain.Api/Dockerfile
- [X] T004 Wire ProductDomain media storage settings and durable local volume in src/CookieCommerce.AppHost/Program.cs
- [X] T005 [P] Add shared product media model folders in src/CookieCommerce.Web/Models/Products/ and src/CookieCommerce.ProductDomain.Api/Models/Products/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain, media, API, projection, and Web-service foundations that MUST be complete before ANY user story can be implemented.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T006 [P] Define ProductMediaRole and ProductMediaAsset value concepts in src/CookieCommerce.ProductDomain.Domain/ValueObjects/ProductMediaAsset.cs
- [X] T007 [P] Define ProductVariant child entity/value concept in src/CookieCommerce.ProductDomain.Domain/ValueObjects/ProductVariant.cs
- [X] T008 Add variant and media state containers to ProductAggregate in src/CookieCommerce.ProductDomain.Domain/Aggregates/ProductAggregate.cs
- [X] T009 [P] Add domain exceptions for duplicate variants, invalid variant data, invalid media ownership, and main-image cardinality in src/CookieCommerce.ProductDomain.Domain/Exceptions/ProductVariantMediaExceptions.cs
- [X] T010 [P] Add mutable Marten domain event classes for product variants and media in src/CookieCommerce.ProductDomain.Domain/DomainEvents/ProductVariantMediaEvents.cs
- [X] T011 Define ProductMediaAssetReadModel and ProductVariantReadModel in src/CookieCommerce.ProductDomain.Application/Products/ReadModels/ProductMediaReadModels.cs
- [X] T012 Extend ProductDetailsReadModel and ProductListItemReadModel with mainImage and variants in src/CookieCommerce.ProductDomain.Application/Products/ReadModels/ProductDetailsReadModel.cs and src/CookieCommerce.ProductDomain.Application/Products/ReadModels/ProductListItemReadModel.cs
- [X] T013 [P] Define IProductMediaStore and ProductMediaOptions abstractions in src/CookieCommerce.ProductDomain.Application/Products/Media/IProductMediaStore.cs
- [X] T014 Implement filesystem-backed ProductDomain media store in src/CookieCommerce.ProductDomain.Infrastructure/Media/FileSystemProductMediaStore.cs
- [X] T015 Add image validation service for content type, size, checksum, and decodability in src/CookieCommerce.ProductDomain.Infrastructure/Media/ProductImageValidator.cs
- [X] T016 Register media store, options, validators, and static file/media URL serving in src/CookieCommerce.ProductDomain.Infrastructure/DependencyInjection/ProductMediaServiceCollectionExtensions.cs and src/CookieCommerce.ProductDomain.Api/Program.cs
- [X] T017 Extend product projections for variant and media events in src/CookieCommerce.ProductDomain.Infrastructure/Projections/ProductProjection.cs
- [X] T018 Extend ProductDomain API response DTOs and mapping for mainImage and variants in src/CookieCommerce.ProductDomain.Api/Models/Products/ProductMapper.cs
- [X] T019 Map new ProductDomain exceptions to ProblemDetails/ValidationProblem responses in src/CookieCommerce.ProductDomain.Api/Middleware/DomainExceptionMiddleware.cs
- [X] T020 Extend Web product DTOs for media and variants in src/CookieCommerce.Web/Models/Products/ProductModels.cs
- [X] T021 Extend Web typed product service contracts for variant and media capabilities in src/CookieCommerce.Web/Services/IBackofficeService.cs

**Checkpoint**: Foundation ready; user story implementation can now begin in parallel if staffed.

---

## Phase 3: User Story 1 - Create product variants from the existing backoffice page (Priority: P1) 🎯 MVP

**Goal**: Backoffice users can add one or more valid variants from the existing product page and see them after product reload.

**Independent Test**: Open `/backoffice/products/`, create or edit a product, add distinct variants, save, reopen, and confirm all variants are visible; duplicate or incomplete variants show row-specific validation.

### Tests for User Story 1 ⚠️

- [X] T022 [P] [US1] Add aggregate tests for adding multiple variants and rejecting duplicates in src/CookieCommerce.ProductDomain.Tests/DomainTests/ProductAggregate_AddVariant_Tests.cs
- [X] T023 [P] [US1] Add handler validation tests for required variant fields and duplicate names in src/CookieCommerce.ProductDomain.Tests/ApplicationTests/UpsertProductVariantsHandler_Tests.cs
- [X] T024 [P] [US1] Add projection tests for ProductVariantAdded/ProductVariantUpdated events in src/CookieCommerce.ProductDomain.Tests/ProjectionsTests/ProductProjection_Variants_Tests.cs
- [X] T025 [P] [US1] Add API contract tests for create/update product variants in src/CookieCommerce.ProductDomain.Tests/IntegrationTests/ProductController_Variants_Tests.cs
- [X] T026 [P] [US1] Add Web component/service tests for variant row validation and form-state preservation in src/CookieCommerce.Web.Tests/Backoffice/ProductsArea/ProductVariants_Tests.cs

### Implementation for User Story 1

- [X] T027 [P] [US1] Add AddVariant/UpdateVariant/RemoveVariant aggregate methods in src/CookieCommerce.ProductDomain.Domain/Aggregates/ProductAggregate.cs
- [X] T028 [P] [US1] Add product variant command/query records in src/CookieCommerce.ProductDomain.Application/Products/Commands/UpsertProductVariants.cs
- [X] T029 [US1] Implement async product variant handlers with cancellation tokens in src/CookieCommerce.ProductDomain.Application/Products/Handlers/UpsertProductVariantsHandler.cs
- [X] T030 [US1] Extend ProductController create/update variant endpoints or request handling in src/CookieCommerce.ProductDomain.Api/Controllers/ProductController.cs
- [X] T031 [US1] Extend CreateProductRequest and UpdateProductRequest with variant DTOs in src/CookieCommerce.ProductDomain.Api/Models/Products/CreateProductRequest.cs and src/CookieCommerce.ProductDomain.Api/Models/Products/UpdateProductRequest.cs
- [X] T032 [US1] Implement Web typed service methods for product variants in src/CookieCommerce.Web/Services/BackofficeService.cs
- [X] T033 [US1] Add variant editing UI to existing products page components in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor and src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryItem.razor
- [X] T034 [US1] Update create-product modal variant flow to persist ProductDomain variants in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/CreateNewProductModal.razor and src/CookieCommerce.Web/Components/Backoffice/ProductsArea/CreateProductModalArea/AddVariantToProductModal.razor

**Checkpoint**: User Story 1 is fully functional and independently testable as MVP.

---

## Phase 4: User Story 2 - Add photos to product variants (Priority: P2)

**Goal**: Backoffice users can upload one or more validated images for each variant, and each image remains scoped to the intended variant after save/reload.

**Independent Test**: Add different photos to two variants, save, reload, and confirm each photo appears only under the correct variant; invalid uploads show localized validation without clearing unrelated fields.

### Tests for User Story 2 ⚠️

- [X] T035 [P] [US2] Add aggregate tests for adding/removing variant photos and preventing cross-variant ownership in src/CookieCommerce.ProductDomain.Tests/DomainTests/ProductAggregate_VariantPhotos_Tests.cs
- [X] T036 [P] [US2] Add media validation tests for unsupported, oversized, and unreadable files in src/CookieCommerce.ProductDomain.Tests/ApplicationTests/ProductImageValidator_Tests.cs
- [X] T037 [P] [US2] Add API multipart upload and delete tests for variant photos in src/CookieCommerce.ProductDomain.Tests/IntegrationTests/ProductController_VariantPhotos_Tests.cs
- [X] T038 [P] [US2] Add projection tests for VariantPhotoAdded/VariantPhotoRemoved in src/CookieCommerce.ProductDomain.Tests/ProjectionsTests/ProductProjection_VariantPhotos_Tests.cs
- [X] T039 [P] [US2] Add Web tests for variant photo previews, validation messages, and form-state preservation in src/CookieCommerce.Web.Tests/Backoffice/ProductsArea/VariantPhotoUpload_Tests.cs

### Implementation for User Story 2

- [X] T040 [P] [US2] Add AddVariantPhoto/RemoveVariantPhoto aggregate methods in src/CookieCommerce.ProductDomain.Domain/Aggregates/ProductAggregate.cs
- [X] T041 [P] [US2] Add variant photo commands in src/CookieCommerce.ProductDomain.Application/Products/Commands/VariantPhotoCommands.cs
- [X] T042 [US2] Implement upload/remove variant photo handlers using IProductMediaStore in src/CookieCommerce.ProductDomain.Application/Products/Handlers/VariantPhotoHandlers.cs
- [X] T043 [US2] Add multipart API endpoints for variant photo upload/delete in src/CookieCommerce.ProductDomain.Api/Controllers/ProductController.cs
- [X] T044 [US2] Add ProductMediaAssetResponse mapping for variant photos in src/CookieCommerce.ProductDomain.Api/Models/Products/ProductMediaAssetResponse.cs
- [X] T045 [US2] Implement Web typed service upload/remove methods for variant photos in src/CookieCommerce.Web/Services/BackofficeService.cs
- [X] T046 [US2] Add variant photo upload, preview, validation, remove, and retry UI in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor
- [X] T047 [US2] Add scoped CSS for responsive/accessible variant media controls in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor.css

**Checkpoint**: User Stories 1 and 2 both work independently; variant photos are isolated by variant.

---

## Phase 5: User Story 3 - Add one main image at the product level (Priority: P3)

**Goal**: Backoffice users can upload, replace, and remove exactly one product-level main image during create/update without affecting variant photos.

**Independent Test**: Upload a main image for a product, save/reload, replace it with another image, save/reload, and confirm exactly one main image exists while variant photos remain unchanged.

### Tests for User Story 3 ⚠️

- [X] T048 [P] [US3] Add aggregate tests for setting, replacing, and removing one main image in src/CookieCommerce.ProductDomain.Tests/DomainTests/ProductAggregate_MainImage_Tests.cs
- [X] T049 [P] [US3] Add API multipart upload/delete tests for product main image in src/CookieCommerce.ProductDomain.Tests/IntegrationTests/ProductController_MainImage_Tests.cs
- [X] T050 [P] [US3] Add projection tests proving main-image replacement does not alter variant photos in src/CookieCommerce.ProductDomain.Tests/ProjectionsTests/ProductProjection_MainImage_Tests.cs
- [X] T051 [P] [US3] Add Web tests for selected-product name edit and main-image controls in src/CookieCommerce.Web.Tests/Backoffice/ProductsArea/ProductMainImage_Tests.cs

### Implementation for User Story 3

- [X] T052 [P] [US3] Add SetMainImage/RemoveMainImage aggregate methods in src/CookieCommerce.ProductDomain.Domain/Aggregates/ProductAggregate.cs
- [X] T053 [P] [US3] Add product main image commands in src/CookieCommerce.ProductDomain.Application/Products/Commands/ProductMainImageCommands.cs
- [X] T054 [US3] Implement upload/replace/remove main image handlers using IProductMediaStore in src/CookieCommerce.ProductDomain.Application/Products/Handlers/ProductMainImageHandlers.cs
- [X] T055 [US3] Add multipart API endpoints for product main image upload/delete in src/CookieCommerce.ProductDomain.Api/Controllers/ProductController.cs
- [X] T056 [US3] Extend Web typed service methods for main image upload/replace/remove and product name update in src/CookieCommerce.Web/Services/BackofficeService.cs
- [X] T057 [US3] Add product name edit and main-image upload/replace/remove controls before the variant table in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryItem.razor
- [X] T058 [US3] Add scoped CSS for selected-product main-image controls in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryItem.razor.css

**Checkpoint**: User Stories 1, 2, and 3 are independently functional; product main image cardinality is enforced.

---

## Phase 6: User Story 4 - Seed bootstrap products with variants and images (Priority: P4)

**Goal**: Development/demo bootstrap maps existing static variant defaults into ProductDomain products, variants, product main images, and variant photos, and fails clearly on missing image files.

**Independent Test**: Start from empty development database/media store, run bootstrap, open backoffice products, and confirm every product has variants and available images; simulate a missing static image and confirm clear bootstrap failure.

### Implementation for User Story 4

> Constitution note: Do not add automated tests for behavioral orchestration or bootstrapping projects. Validate this story through the implementation tasks and quickstart/manual bootstrap scenarios in Phase 7.

- [X] T062 [P] [US4] Extract static inventory variant defaults into a bootstrap-readable contract in src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs
- [X] T063 [US4] Implement ProductDomain bootstrap variant/media mapping in src/CookieCommerce.DevBootstrap/DevelopmentBootstrapData.cs
- [X] T064 [US4] Add bootstrap media file resolution and fail-fast missing-image validation in src/CookieCommerce.DevBootstrap/DevelopmentBootstrapService.cs
- [X] T065 [US4] Copy or register default static image assets into configured ProductDomain media storage in src/CookieCommerce.DevBootstrap/DevelopmentBootstrapService.cs
- [X] T066 [US4] Ensure bootstrapped product main images and variant photos are visible through Web models in src/CookieCommerce.Web/Services/Products/StaticInventoryList.cs and src/CookieCommerce.Web/Models/Products/ProductModels.cs

**Checkpoint**: Bootstrap data validates, seeds variants/images, and displays through the existing backoffice product page.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final quality, documentation, accessibility, and operational checks across all user stories.

- [X] T067 [P] Update Web service interface documentation for product variants and media in Documentation/backend-documentation.md
- [X] T068 [P] Add feature implementation notes and manual validation references in specs/005-product-variant-images/quickstart.md
- [X] T069 Run accessibility and responsive cleanup for product/variant media UI in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/Products.razor and src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryItem.razor
- [X] T070 Verify ProductDomain media logging avoids file contents/secrets and uses structured messages in src/CookieCommerce.ProductDomain.Infrastructure/Media/FileSystemProductMediaStore.cs
- [X] T071 Build and verify ProductDomain API container/AppHost registration with src/CookieCommerce.ProductDomain.Api/Dockerfile and src/CookieCommerce.AppHost/Program.cs
- [X] T072 Run quickstart scenarios 1-7 and record any intentionally deferred issues in specs/005-product-variant-images/quickstart.md
- [X] T073 Run `dotnet build src` from repository root and fix build errors in src/
- [X] T074 Run `dotnet test src/CookieCommerce.ProductDomain.Tests` and `dotnet test src/CookieCommerce.Web.Tests` from repository root and fix failures in src/
- [X] T075 Run `dotnet test src` from repository root for broad domain/API/Web regression coverage and fix failures in src/
- [X] T076 Run `dotnet format` from repository root and fix formatting violations in src/

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion; blocks all user stories.
- **User Stories (Phase 3+)**: All depend on Foundational completion.
- **Polish (Phase 7)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2; MVP and no dependency on other stories.
- **User Story 2 (P2)**: Starts after Phase 2 and can be built against foundational variant/media concepts, but final UI value is highest after US1 variant rows exist.
- **User Story 3 (P3)**: Starts after Phase 2; independent from US2 because product main images are separate from variant photos.
- **User Story 4 (P4)**: Starts after US1, US2, and US3 behaviors exist because bootstrap depends on variants, variant photos, and main images.

### Within Each User Story

- Tests should be written before or alongside implementation and should fail for the intended reason before implementation when practical.
- Domain aggregate methods and events before application handlers.
- Application handlers before API endpoints.
- API/Web models and typed services before Razor component integration.
- Projection/read-model updates before reload-based acceptance validation.
- Story complete before moving to the next priority when working sequentially.

---

## Parallel Opportunities

- Setup tasks T002, T003, and T005 can run in parallel after T001 starts.
- Foundational tasks T006, T007, T009, T010, and T013 can run in parallel because they touch separate files.
- Tests within each user story marked [P] can run in parallel.
- US2 media upload implementation and US3 main-image implementation can proceed in parallel after Phase 2 if developers coordinate changes to ProductController and BackofficeService.
- Web CSS tasks T047 and T058 can run in parallel with backend API work after component contracts are known.
- Documentation tasks T067 and T068 can run in parallel with final validation tasks T073-T076.

## Parallel Example: User Story 1

```bash
# Launch tests for User Story 1 together:
Task: "T022 [US1] Add aggregate tests in src/CookieCommerce.ProductDomain.Tests/DomainTests/ProductAggregate_AddVariant_Tests.cs"
Task: "T023 [US1] Add handler validation tests in src/CookieCommerce.ProductDomain.Tests/ApplicationTests/UpsertProductVariantsHandler_Tests.cs"
Task: "T024 [US1] Add projection tests in src/CookieCommerce.ProductDomain.Tests/ProjectionsTests/ProductProjection_Variants_Tests.cs"
Task: "T025 [US1] Add API contract tests in src/CookieCommerce.ProductDomain.Tests/IntegrationTests/ProductController_Variants_Tests.cs"
Task: "T026 [US1] Add Web component/service tests in src/CookieCommerce.Web.Tests/Backoffice/ProductsArea/ProductVariants_Tests.cs"

# Launch independent implementation work after tests are defined:
Task: "T027 [US1] Add aggregate variant methods in src/CookieCommerce.ProductDomain.Domain/Aggregates/ProductAggregate.cs"
Task: "T028 [US1] Add variant commands in src/CookieCommerce.ProductDomain.Application/Products/Commands/UpsertProductVariants.cs"
```

## Parallel Example: User Story 2

```bash
# Launch tests for User Story 2 together:
Task: "T035 [US2] Add aggregate tests in src/CookieCommerce.ProductDomain.Tests/DomainTests/ProductAggregate_VariantPhotos_Tests.cs"
Task: "T036 [US2] Add media validation tests in src/CookieCommerce.ProductDomain.Tests/ApplicationTests/ProductImageValidator_Tests.cs"
Task: "T037 [US2] Add API multipart tests in src/CookieCommerce.ProductDomain.Tests/IntegrationTests/ProductController_VariantPhotos_Tests.cs"
Task: "T039 [US2] Add Web upload tests in src/CookieCommerce.Web.Tests/Backoffice/ProductsArea/VariantPhotoUpload_Tests.cs"

# Launch independent UI/style work after service contracts are known:
Task: "T046 [US2] Add variant photo UI in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor"
Task: "T047 [US2] Add variant photo CSS in src/CookieCommerce.Web/Components/Backoffice/ProductsArea/SelectedInventoryVariant.razor.css"
```

## Parallel Example: User Story 3

```bash
# Launch tests for User Story 3 together:
Task: "T048 [US3] Add aggregate tests in src/CookieCommerce.ProductDomain.Tests/DomainTests/ProductAggregate_MainImage_Tests.cs"
Task: "T049 [US3] Add API tests in src/CookieCommerce.ProductDomain.Tests/IntegrationTests/ProductController_MainImage_Tests.cs"
Task: "T050 [US3] Add projection tests in src/CookieCommerce.ProductDomain.Tests/ProjectionsTests/ProductProjection_MainImage_Tests.cs"
Task: "T051 [US3] Add Web tests in src/CookieCommerce.Web.Tests/Backoffice/ProductsArea/ProductMainImage_Tests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational.
3. Complete Phase 3: User Story 1.
4. Stop and validate User Story 1 independently via `/backoffice/products/` and targeted ProductDomain/Web tests.
5. Demo/deploy MVP if variant creation and reload behavior are acceptable.

### Incremental Delivery

1. Setup + Foundational: establish ProductDomain variant/media model, read-model shape, media store seam, API DTO shape, and Web service contracts.
2. US1: create/review variants from the existing backoffice page.
3. US2: add/remove variant photos with validation and variant scoping.
4. US3: add/replace/remove the single product main image and selected-product name edit.
5. US4: seed bootstrap products with static variants and images.
6. Polish: documentation, accessibility, AppHost/container verification, quickstart scenarios, build/test/format.

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1 and Phase 2 together.
2. After Phase 2:
   - Developer A: US1 variant domain/application/API/Web flow.
   - Developer B: US2 variant photo storage/API/UI flow.
   - Developer C: US3 product main image storage/API/UI flow.
3. After US1-US3 are complete, Developer D or the team completes US4 bootstrap mapping.
4. Everyone participates in Phase 7 validation and fixes.

---

## Notes

- Keep controllers thin and delegate to MediatR/application handlers.
- Keep ProductAggregate persistence/media-store ignorant; store only media references and metadata in events/read models.
- Do not store image bytes in Marten events or product text fields.
- Use async handlers and pass CancellationToken through application, infrastructure, and API operations.
- Do not create a separate variant-management page; use existing ProductsArea components.
- Do not call backend APIs directly from Razor components; route all backend access through typed Web services under src/CookieCommerce.Web/Services/.
- Update Documentation/backend-documentation.md whenever src/CookieCommerce.Web/Services/**/*.cs interfaces change.
- Avoid logging file content, secrets, or developer-specific absolute paths.
- Stop at checkpoints to validate each story independently.
