# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[Unreleased]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.3.1...HEAD
[1.3.1]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.3.0...v1.3.1
[1.3.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.1.5...v1.2.0
[1.1.5]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.1.4...v1.1.5
[1.1.4]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.0.0...v1.1.4
[1.0.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/releases/tag/v1.0.0
