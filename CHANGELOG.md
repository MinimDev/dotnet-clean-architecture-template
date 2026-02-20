# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.1.5...v1.2.0
[1.1.5]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.1.4...v1.1.5
[1.1.4]: https://github.com/MinimDev/dotnet-clean-architecture-template/compare/v1.0.0...v1.1.4
[1.0.0]: https://github.com/MinimDev/dotnet-clean-architecture-template/releases/tag/v1.0.0
