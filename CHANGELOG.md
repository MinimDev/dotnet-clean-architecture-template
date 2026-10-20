# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.7.0-rc2] - 2026-10-20

### Added
- **In-App Notification System** — Centralized notification infrastructure supporting targeted user alerts and system-wide broadcast events:
  - `Notification` entity in `CleanArchitecture.Domain.Entities` with `Title`, `Message`, `Type` (Info, Success, Warning, Error), `UserId` (null for broadcast), `IsRead`, `ActionUrl`, `CreatedAt`, and `ReadAt`.
  - `NotificationCreatedEvent` domain event dispatched upon creation.
  - `INotificationService` abstraction in Application layer with `SendAsync`, `BroadcastAsync`, `MarkAsReadAsync`, `MarkAllAsReadAsync`, and `GetUnreadCountAsync`.
  - `NotificationService` implementation in `Infrastructure.Shared` registered as scoped service.
  - MediatR CQRS features: `GetNotificationsQuery` (with pagination & unread filter), `GetUnreadCountQuery`, `MarkNotificationAsReadCommand`, and `MarkAllNotificationsAsReadCommand`.
  - `NotificationsController` in WebAPI with endpoints for retrieving notifications, unread count, and marking notifications as read.
  - Event-driven integrations:
    - MassTransit `ProductCreatedConsumer` automatically broadcasts in-app notification when a new product is added.
    - Hangfire `ExampleJob` automatically broadcasts in-app notification upon background task completion.
  - Modern WebUI Notification Panel & Topbar Bell:
    - Notification bell icon in `MainLayout.razor` with dynamic unread badge counter.
    - Glassmorphism slide-down `NotificationPanel.razor` with relative time ("Just now", "5m ago"), unread indicators, type icons, and one-click mark-as-read with auto-navigation.
    - Periodic background unread polling (every 30s) when user is authenticated.
  - Full automated test coverage: `GetNotificationsQueryValidatorTests` (unit) and `NotificationsControllerTests` (integration).

## [1.7.0-rc1] - 2026-10-20

### Added
- **Dynamic App Settings & Feature Flags Management** — Runtime application configuration and feature toggles stored in the database with caching, fallback, and administrative controls:
  - `AppSetting` domain entity in `CleanArchitecture.Domain.Entities` inheriting `BaseAuditableEntity`, encapsulating `Key`, `Value`, `DataType` (string/boolean/number), `Description`, `Group` (General/Features/Security/System), and `IsReadOnly` safety guard.
  - `AppSettingChangedEvent` domain event dispatched when configuration values change.
  - `IAppSettingsService` abstraction in Application layer with typed `GetAsync<T>`, `IsFeatureEnabledAsync`, and `SetAsync` methods.
  - `DbAppSettingsService` implementation in `Infrastructure.Shared` with in-memory caching (`IMemoryCache`), automatic cache invalidation upon updates, and transparent fallback to `IConfiguration` (`appsettings.json`).
  - MediatR CQRS features: `GetAppSettingsQuery`, `GetAppSettingByKeyQuery`, and `UpdateAppSettingCommand` with `UpdateAppSettingCommandValidator`.
  - EF Core configuration `AppSettingConfiguration` with unique index on `Key`, index on `Group`, and seeded default settings (`General.ApplicationName`, `General.SupportEmail`, `Features.ProductExport.Enabled`, `Features.AuditLogging.Enabled`, `Features.Registration.Enabled`, `Security.MaxFailedAccessAttempts`, `Security.SessionTimeoutMinutes`, `System.TemplateVersion`).
  - `AppSettingsController` in WebAPI with `[Authorize(Roles = "Admin")]` and rate limiting.
  - Interactive Admin WebUI page (`AppSettings.razor`) featuring:
    - Group category filtering chips (`All`, `General`, `Features`, `Security`, `System`)
    - Real-time search by setting key or description
    - Dynamic editor controls based on `DataType`: instant `MudSwitch` toggles for booleans, `MudTextField` with save buttons for numbers and strings
    - Lock indicators and disabled inputs for `IsReadOnly` settings
    - Unsaved changes indicator and instant snackbar feedback
    - Sidebar link in `NavMenu.razor` under the System menu for Administrators
  - Full automated test coverage: `UpdateAppSettingCommandValidatorTests` (unit) and `AppSettingsControllerTests` (integration verifying CRUD, authorization, read-only guards, data type validation, and automatic Phase 2 audit trail logging).

## [1.7.0-beta] - 2026-10-19

### Added
- **Automated Audit Trail System** — Comprehensive change tracking across all domain entities without polluting handlers or domain logic:
  - `AuditLog` entity in `CleanArchitecture.Domain.Entities` storing `EntityName`, `Action` (Created, Updated, Deleted), `PrimaryKey`, `OldValues` (JSON), `NewValues` (JSON), `AffectedColumns`, `UserId`, `UserName`, and UTC `Timestamp`.
  - `IApplicationDbContext.AuditLogs` DbSet registration with indexed columns (`EntityName`, `Action`, `Timestamp`, `UserId`).
  - Extended `AuditableEntityInterceptor` in `Infrastructure.Persistence` to automatically capture inserted, updated, and soft-deleted entities into `AuditLogs` in the exact same atomic database transaction.
  - EF Core migration `AddAuditLogs` with cross-provider support (SQL Server and SQLite).
  - MediatR query `GetAuditLogsQuery` with filtering (`EntityName`, `Action`, `UserId`, date range) and pagination via `PaginatedList<AuditLogDto>`.
  - `AuditLogsController` in WebAPI with `[Authorize(Roles = "Admin")]` and rate limiting.
  - Interactive, modern WebUI page (`AuditLogs.razor`) featuring:
    - Real-time action filtering (All, Created, Updated, Deleted chips)
    - Side-by-side collapsible visual diff viewer (Old Values strikethrough vs New Values highlight)
    - Primary key and affected columns badges
    - Skeleton loading and empty state illustrations
    - Sidebar link in `NavMenu.razor` under the System menu for Administrators
  - Full automated test coverage: `GetAuditLogsQueryValidatorTests` (unit) and `AuditLogsControllerTests` (integration).

## [1.7.0-alpha] - 2026-10-19

### Added
- **Idempotency Key Support** — MediatR pipeline behaviour (`IdempotencyBehaviour<TRequest, TResponse>`) to prevent duplicate transactions (double-submit, network retries):
  - `IIdempotencyService` in Application layer for caching responses with configurable TTL (`Idempotency:CacheDurationHours`, default 24h).
  - `IIdempotencyKeyAccessor` abstraction to isolate ASP.NET Core `HttpContext` from the Application layer.
  - `HttpIdempotencyKeyAccessor` in WebAPI layer extracting `Idempotency-Key` header from HTTP requests.
  - `IdempotencyService` implementation using `IMemoryCache` in `Infrastructure.Shared`.
  - Swagger/Scalar documentation via `IdempotencyKeyOperationFilter` showing the optional `Idempotency-Key` header on all mutating endpoints (`POST`, `PUT`, `DELETE`, `PATCH`).
  - Unit tests in `CleanArchitecture.Application.UnitTests` (`IdempotencyBehaviourTests`).
  - End-to-end integration test in `CleanArchitecture.Application.IntegrationTests` verifying duplicate submission prevention.

### Changed
- **Blazor WebUI Double-Submit Protection**:
  - `CreateProductDialog.razor`: Guard `if (_isSaving) return;` at the very first line before validation to prevent rapid double-click submissions.
  - `EditProductDialog.razor`: Double-submit prevention with loading feedback.
  - `ProductsClient.cs`: Automatically injects a unique `Idempotency-Key` header per mutation request.
- **Integration Test Infrastructure**:
  - Fixed database provider clash between SQL Server and SQLite in `CustomWebApplicationFactory`.
  - Relaxed rate limiting in `Program.cs` during `Testing` environment to prevent 429 false-positives in automated test suites.
  - Added `[JsonConstructor]` to `Result<T>` and parameterless constructor to `PaginatedList<T>` for seamless JSON deserialization.

## [1.6.1] - 2026-08-28

### Changed
- **Hangfire Integration** — Refactored Hangfire background job examples to properly follow Clean Architecture. 
  - Extracted job definitions to `IExampleJob` in the Application layer.
  - Placed job logic inside `ExampleJob` in the Shared Infrastructure layer.
  - Added step-by-step Hangfire implementation documentation in `README.md`.

## [1.6.0] - 2026-08-13

### Added
- **MassTransit Message Bus** — New `CleanArchitecture.Infrastructure.Messaging` project integrating MassTransit v8
  - **Transport switching** via single config key `"MassTransit:Transport"`:
    - `"InMemory"` *(default)* — no external broker, runs anywhere out of the box
    - `"RabbitMQ"` — AMQP production/staging broker (Management UI at `:15672`)
    - `"ActiveMQ"` — Enterprise broker (Admin UI at `:8161`)
  - **Enable/disable toggle** — set `"MassTransit:Enabled": false` to opt-out with zero code changes
  - **Auto-discovery** — all `IConsumer<T>` classes in `Infrastructure.Messaging` assembly are registered automatically
  - **`IEventBus` abstraction** in Application layer — keeps handlers fully decoupled from MassTransit
  - **`ProductCreatedMessage`** integration message contract as canonical example in `Application/Common/Messages/`
  - **`ProductCreatedConsumer`** — demonstrates the Bus → Hangfire handoff pattern (loose coupling)
  - **`MassTransitEventBus`** — thin `IPublishEndpoint` wrapper implementing `IEventBus`

### Changed
- `CreateProductCommandHandler` — injects `IEventBus` and publishes `ProductCreatedMessage` after successful `SaveChangesAsync`
- `docker-compose.yml` — added commented-out RabbitMQ and ActiveMQ service definitions (uncomment to use)
- `Dockerfile` — added `COPY` for new `Infrastructure.Messaging.csproj` in build stage
- `appsettings.json` — added `MassTransit` config section with all transport options and inline documentation
- `README.md` — documented MassTransit feature, architecture layer, consumer guide, and transport switching

### Architecture Notes
- **Domain Event (MediatR)** ≠ **Integration Message (MassTransit)**:
  - `IDomainEvent` + MediatR = in-process, same transaction, in-memory only
  - `IEventBus` + MassTransit = async, cross-consumer, transport-agnostic
- **Hangfire + MassTransit integration**: Consumer receives message → hands off to Hangfire for persistent, retry-able jobs

## [1.5.3] - 2026-05-26

### Changed
- Minor updates and bug fixes.
- General template improvements.

## [1.5.2] - 2026-04-29

### Changed
- **Swashbuckle.AspNetCore v10** — Updated swagger configuration syntax (`OpenApiSecuritySchemeReference`) to support version 10 breaking changes.
- **Scalar API Reference UI** — Hosted `scalar.js` locally instead of relying on external CDN to ensure offline compatibility and avoid rate-limiting.
- **Hangfire** — Fixed `Hangfire` integration test initialization to prevent server startup crashes in the `Testing` environment.

## [1.5.1] - 2026-04-17

### Fixed
- **MudBlazor v9 breaking change** — `Typography` properties (`FontWeight`) changed from `int` to `string`; updated all heading/button/subtitle properties in `MainLayout.razor`
- **Swashbuckle.AspNetCore v10 → v9** — `Microsoft.OpenApi` v3.x removed the `Microsoft.OpenApi.Models` namespace; downgraded to Swashbuckle v9.0.1 which is compatible with the existing `ConfigureSwaggerOptions.cs`
- **Scalar API Reference UI** — `Scalar.AspNetCore` v2.14 bundles the wrong version of `scalar.js` (standalone auto-init instead of programmatic API), causing white screen. Replaced NuGet package with a lightweight CDN-based custom endpoint (`GET /scalar/{documentName}`) that calls `Scalar.createApiReference` directly

## [1.5.0] - 2026-04-16

### Added
- **Refresh Token** system — `POST /api/v1/Auth/refresh` and `POST /api/v1/Auth/revoke` endpoints
- `RefreshToken` entity persisted in IdentityDB with unique index and cascade delete
- Automatic token rotation: every refresh issues a new pair and invalidates the old one
- `AccessTokenExpiryMinutes` (default: 15 min) and `RefreshTokenExpiryDays` (default: 7 days) config keys
- **Integration Tests** scaffold with `CustomWebApplicationFactory` (SQLite in-memory, no SQL Server required)
- Auth integration tests: register, login, refresh rotation, invalid/expired token cases
- Products integration tests: unauthenticated guard, JWT auth, Admin vs Member RBAC

### Changed
- `TokenService` refactored — now accepts `IdentityDbContext` to persist refresh tokens
- `AuthResponse` renamed field `Token` → `AccessToken`; added `RefreshToken` field
- `AuthorizationMessageHandler` in WebUI now intercepts 401 and performs silent refresh automatically
- `AuthClient.LogoutAsync()` now revokes the refresh token server-side before clearing local state
- `TokenProvider` extended: stores `RefreshToken` + `UpdateAccessToken()` for silent refresh flow
- JWT configuration: `ExpirationInDays` replaced with `AccessTokenExpiryMinutes` + `RefreshTokenExpiryDays`

### Migration
```bash
dotnet ef database update \
  --project "src/Infrastructure/CleanArchitecture.Infrastructure.Identity" \
  --startup-project "src/Presentation/CleanArchitecture.WebAPI" \
  --context IdentityDbContext
```

## [1.4.0] - 2026-04-06

### Added
- Integrated Mapster mapping configurations (`IRegister`) with Auto-scan mechanism in Dependency Injection
- Example custom mapping implementation (`ProductMappingConfig.cs`) projecting `Price` to `PriceDisplay` string
- Aesthetic WebUI overhaul (Glassmorphism, dynamic mesh gradients, modern typography using Outfit/Inter, and interactive animations)
- `MultiSelection` support and advanced UI layout for User Management Dashboard (`Users.razor`)
- Custom Badges and Dot Indicators for user roles with premium SaaS look

### Changed
- Refactored `GetProductsListQueryHandler` and `GetProductByIdQueryHandler` to utilize Mapster's `ProjectToType<T>()` instead of manual mapping
- Refactored `UsersController.GetUsers` to utilize Mapster's `.Adapt<List<UserDto>>()` mapping pattern
- Improved Snackbar configuration: transitioned to `TopRight` position with non-duplicate and auto-close settings
- Refined Sidebar navigation with gradient active-states and premium accents

### Fixed
- Resolved `InvalidOperationException` regarding `IAuthenticationService` by migrating from page-level `@attribute [Authorize]` to component-level `<AuthorizeView>`
- Corrected child content parameter name ambiguity in `Users.razor` (Authentication Context vs DataGrid Context)

## [1.3.1] - 2026-04-06

### Fixed
- JWT auth and WebUI fixes

## [1.3.0] - 2026-02-22
### Added
- Role-Based Access Control (RBAC) implementation (Admin/Member roles)
- User Management Dashboard in WebUI (View list and manage roles dynamically)
- `SmtpEmailService` and `IEmailService` for standardized email sending
- Interactive Edit, Delete, and Details Modal Dialogs for Products in WebUI

### Changed
- Converted Products table (`MudDataGrid`) to a responsive layout using `MudCard` with hover animations and skeleton loading

### Fixed
- Fixed compilation error regarding generic definition of `Result<T>` in WebUI architecture

## [1.2.0] - 2026-02-20

### Added
- Visual Studio 2022 full support via `.template.config/ide.host.json` (`createInPlace=true`)
- GitHub Actions CI workflow — automated build and test on every push/PR
- GitHub Actions Publish workflow — auto-publish to NuGet on version tag
- `LICENSE` file (MIT)
- `CHANGELOG.md`
- GitHub Stars and Forks badges in README
- Contributing guide with Fork → Branch → PR workflow

### Changed
- `publish.ps1` — removed hardcoded API key, replaced with `-ApiKey` parameter and confirmation prompt
- README restructured: separated Quick Start for template users vs contributors
- Improved file exclusions in `template.json` (exclude `*.csproj`, `publish.ps1`, log files)
- `primaryOutputs` in `template.json` updated with host conditions

### Fixed
- Visual Studio creating empty solution when using template — resolved with `ide.host.json`

## [1.1.5] - 2026-02-18

### Added
- Unit tests for `UpdateProduct` command and validator
- Integration test project scaffolding

### Changed
- Updated README with latest project structure

## [1.1.4] - 2026-02-10

### Added
- `UpdateProduct` command with FluentValidation
- Unit of Work pattern
- OpenTelemetry integration for distributed tracing
- Serilog file sink configuration

### Changed
- Separated Identity and Application databases
- Improved Scalar UI JWT Bearer authentication flow

## [1.0.0] - 2026-01-20

### Added
- Initial release
- Clean Architecture solution structure (Domain, Application, Infrastructure, Presentation)
- CQRS with MediatR
- Entity Framework Core with SQL Server
- ASP.NET Core Identity with JWT Bearer authentication
- FluentValidation pipeline behavior
- AutoMapper
- Scalar UI API documentation
- Health checks
- Soft delete and audit trail
- Docker support
- xUnit test projects

[Unreleased]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.5.3...HEAD
[1.5.3]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.5.2...v1.5.3
[1.5.2]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.5.1...v1.5.2
[1.5.1]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.3.1...v1.4.0
[1.3.1]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.3.0...v1.3.1
[1.3.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.1.5...v1.2.0
[1.1.5]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.1.4...v1.1.5
[1.1.4]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.0.0...v1.1.4
[1.0.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/releases/tag/v1.0.0
