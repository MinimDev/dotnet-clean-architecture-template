# Clean Architecture Template Solution (.NET 10)

A comprehensive, production-ready **ASP.NET Core** starter template implementing **Clean Architecture** principles with CQRS, MediatR, JWT Authentication, and modern API documentation.

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4.svg?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/download)
[![Mapster](https://img.shields.io/badge/Mapping-Mapster_v10-00AFF0?style=flat-square)](https://github.com/MapsterMapper/Mapster)
[![NuGet](https://img.shields.io/nuget/v/Minimdev.CleanArchitecture.Template.svg?style=flat-square&logo=nuget&color=004880)](https://www.nuget.org/packages/Minimdev.CleanArchitecture.Template)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Minimdev.CleanArchitecture.Template.svg?style=flat-square&logo=nuget&color=004880)](https://www.nuget.org/packages/Minimdev.CleanArchitecture.Template)
[![GitHub Stars](https://img.shields.io/github/stars/MinimDev/dotnet-clean-architecture-template.svg?style=flat-square&logo=github)](https://github.com/MinimDev/dotnet-clean-architecture-template/stargazers)
[![License: MIT](https://img.shields.io/badge/license-MIT-22c55e.svg?style=flat-square)](LICENSE)

---

## 🏗️ Project Structure

```
YourProject/
├── src/
│   ├── Core/
│   │   ├── YourProject.Domain/                        # Enterprise business rules & entities
│   │   └── YourProject.Application/                   # Use cases, CQRS, validators, interfaces
│   ├── Infrastructure/
│   │   ├── YourProject.Infrastructure.Persistence/    # Data access (EF Core, migrations)
│   │   ├── YourProject.Infrastructure.Identity/       # Authentication & JWT
│   │   └── YourProject.Infrastructure.Shared/         # Shared services (email, datetime, etc.)
│   └── Presentation/
│       ├── YourProject.WebAPI/                        # REST API controllers, middleware, DI
│       └── YourProject.WebUI/                         # Blazor Server interactive frontend
├── tests/
│   ├── YourProject.Domain.UnitTests/
│   ├── YourProject.Application.UnitTests/
│   └── YourProject.Application.IntegrationTests/
├── docker-compose.yml
├── Dockerfile
├── YourProject.sln
└── README.md
```

---

## ✨ Features

### 🏛️ Architecture & Patterns
- ✅ **Clean Architecture** — Clear layer separation with strict dependency rules
- ✅ **CQRS** — Command Query Responsibility Segregation via MediatR
- ✅ **Domain-Driven Design** — Rich domain models encapsulating business logic
- ✅ **Repository Pattern** — Abstracted data access layer
- ✅ **Result Pattern** — Consistent, explicit error handling without exceptions

### ⚙️ Technology Stack
- ✅ **.NET 10** — Latest long-term support framework
- ✅ **Blazor Server WebUI** — Interactive frontend with **MudBlazor** Material Design components
- ✅ **MediatR v12.x** — In-process messaging for CQRS implementation
- ✅ **Entity Framework Core 10** — Code-First ORM with full migration support
- ✅ **SQL Server** — Separate databases for Application and Identity
- ✅ **ASP.NET Core Identity** — User and role management
- ✅ **JWT Bearer Authentication** — Stateless, secure API authentication
- ✅ **Mapster v10.x** — High-performance object-to-object mapping
- ✅ **FluentValidation** — Expressive and testable request validation
- ✅ **Scalar UI** — Modern API documentation with JWT support
- ✅ **OpenTelemetry** — Distributed tracing and observability (Console + OTLP exporters)
- ✅ **xUnit** — Unit and integration testing framework

### 🚀 Key Capabilities
- ✅ **Role-Based Access Control (RBAC)** — Pre-configured `Admin` and `Member` roles with authorization policies
- ✅ **User Management Dashboard** — View users and modify roles dynamically from the UI
- ✅ **API Versioning** — Versioned endpoints (`/api/v1/...`) via URL segment & request header
- ✅ **Rate Limiting** — Built-in ASP.NET Core rate limiting (global & auth-specific limits)
- ✅ **Response Caching** — Output caching configured for GET endpoints
- ✅ **Background Jobs** — Hangfire integration with SQL Server storage & `/hangfire` dashboard
- ✅ **Resilience (Polly)** — Retry and circuit-breaker policies via `Microsoft.Extensions.Http.Resilience`
- ✅ **Email Service** — `IEmailService` abstraction with built-in `SmtpEmailService` implementation
- ✅ **Audit Trails** — Automatic tracking of `CreatedAt` / `ModifiedAt` timestamps
- ✅ **Soft Delete** — Global EF Core query filters for logical record deletion
- ✅ **Health Checks** — Database connectivity monitoring endpoint
- ✅ **Global Exception Handling** — Centralized middleware for consistent error responses
- ✅ **CORS Support** — Configurable cross-origin resource sharing
- ✅ **Docker Support** — Containerization-ready with `Dockerfile` and `docker-compose.yml`
- ✅ **Visual Studio 2022 & 2026** — Full IDE support via the New Project dialog
- ✅ **.editorconfig** — Comprehensive code style and formatting rules

---

## 📦 Using as a Template

### Install from NuGet

```bash
dotnet new install Minimdev.CleanArchitecture.Template
```

### Create a New Project

#### Via CLI

```bash
# Syntax: dotnet new cleanarch -n [YourProjectName] -o [OutputDirectory]
dotnet new cleanarch -n MyProject -o MyProject
```

**What happens automatically on project creation:**

| Original | Replaced With |
|---|---|
| `CleanArchitecture.Domain` | `MyProject.Domain` |
| `namespace CleanArchitecture.Domain` | `namespace MyProject.Domain` |
| `src/Core/CleanArchitecture.Domain/` | `src/Core/MyProject.Domain/` |

#### Via Visual Studio 2022 / 2026

1. Open **Visual Studio 2022** or **Visual Studio 2026**
2. Click **Create a new project**
3. Search for **"Clean Architecture Solution"**
4. Select the template, enter your project name → **Create**

> The template automatically renames **all** projects and namespaces to match your chosen project name.

### Uninstall Template

```bash
dotnet new uninstall Minimdev.CleanArchitecture.Template
```

---

## 🚀 Quick Start

### A. Template User (Generated Project)

After creating your project from the template (`dotnet new cleanarch -n MyProject`), replace `MyProject` with your actual project name in the steps below.

#### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server or SQL Server Express
- Visual Studio 2022 / 2026, or VS Code

#### Setup Steps

**1. Update connection strings**

Edit `src/Presentation/MyProject.WebAPI/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection":  "Server=localhost;Database=MyProjectDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "IdentityConnection": "Server=localhost;Database=MyProjectIdentityDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=MyProjectHangfireDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**2. Apply database migrations**

```bash
# Application DB
dotnet ef database update \
  --project "src/Infrastructure/MyProject.Infrastructure.Persistence" \
  --startup-project "src/Presentation/MyProject.WebAPI" \
  --context ApplicationDbContext

# Identity DB
dotnet ef database update \
  --project "src/Infrastructure/MyProject.Infrastructure.Identity" \
  --startup-project "src/Presentation/MyProject.WebAPI" \
  --context IdentityDbContext
```

**3. Run the application**

```bash
dotnet run --project "src/Presentation/MyProject.WebAPI"
```

**4. Access the application**

| URL | Description |
|---|---|
| `https://localhost:{port}/scalar/v1` | Scalar API Documentation |
| `https://localhost:{port}/health` | Health Check |
| `https://localhost:{port}/hangfire` | Hangfire Job Dashboard |

> [!NOTE]
> The port number depends on how you run the application:
> - **CLI (`dotnet run`)** — port is defined in `Properties/launchSettings.json` (default: `7253` for HTTPS, `5022` for HTTP)
> - **Visual Studio 2022 / 2026** — Visual Studio may assign a different port automatically based on the selected launch profile. Check the **Output** window or browser tab opened by VS for the actual URL.
> - To always use a fixed port, set `applicationUrl` explicitly in `launchSettings.json`.

---

### B. Contributor (Template Source)

> This section is for contributors who want to develop or modify the template itself.

**1. Clone the repository**

```bash
git clone https://github.com/MinimDev/dotnet-clean-architecture-template.git
cd dotnet-clean-architecture-template
```

**2. Update connection strings**

Edit `src/Presentation/CleanArchitecture.WebAPI/appsettings.Development.json` with your local SQL Server details.

**3. Apply migrations**

```bash
dotnet ef database update \
  --project "src/Infrastructure/CleanArchitecture.Infrastructure.Persistence" \
  --startup-project "src/Presentation/CleanArchitecture.WebAPI" \
  --context ApplicationDbContext

dotnet ef database update \
  --project "src/Infrastructure/CleanArchitecture.Infrastructure.Identity" \
  --startup-project "src/Presentation/CleanArchitecture.WebAPI" \
  --context IdentityDbContext
```

**4. Run**

```bash
dotnet run --project "src/Presentation/CleanArchitecture.WebAPI"
```

---

## 📖 API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/Auth/register` | Register a new user |
| `POST` | `/api/v1/Auth/login` | Login and receive a JWT token *(Rate limited: 10 req/min)* |

### Products *(Requires Authentication)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET`    | `/api/v1/Products`      | Get paginated list *(Output Cached: 30s)* |
| `GET`    | `/api/v1/Products/{id}` | Get product by ID *(Output Cached: 60s)* |
| `POST`   | `/api/v1/Products`      | Create a new product |
| `PUT`    | `/api/v1/Products/{id}` | Update an existing product |
| `DELETE` | `/api/v1/Products/{id}` | Delete a product |

### Background Jobs *(Requires Authentication)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/Jobs/fire-and-forget` | Enqueue an immediate background task |
| `POST` | `/api/v1/Jobs/delayed`         | Schedule a delayed background task |
| `POST` | `/api/v1/Jobs/recurring`       | Register a CRON-based recurring task |
| `POST` | `/api/v1/Jobs/recurring/trigger` | Trigger the daily recurring task immediately |

**Hangfire Dashboard:** `https://localhost:7253/hangfire`

### Authenticating in Scalar UI

1. Call `POST /api/v1/Auth/login` and copy the returned JWT token
2. In **Scalar UI**, click **"Auth Type"** → select **"Bearer"**
3. Paste your token *(without the "Bearer" prefix)*

---

## 🐳 Docker Support

```bash
# Build and start all services (API + SQL Server)
docker-compose up -d

# Stop and remove containers
docker-compose down
```

The API will be available at `http://localhost:8080`.

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

---

## 🔧 Configuration Reference

Key sections in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection":  "Server=localhost;Database=CleanArchitectureDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "IdentityConnection": "Server=localhost;Database=CleanArchitectureIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=CleanArchitectureHangfireDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "your-very-strong-secret-key-min-32-chars",
    "Issuer": "CleanArchitecture",
    "Audience": "CleanArchitectureUsers",
    "ExpirationInDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:5001"]
  },
  "RateLimiting": {
    "WindowSeconds": 60,
    "PermitLimit": 100,
    "Auth": {
      "WindowSeconds": 60,
      "PermitLimit": 10
    }
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "your-email@gmail.com",
    "DisplayName": "Clean Architecture App"
  }
}
```

**Recommended environment variables for production overrides:**

```bash
ConnectionStrings__DefaultConnection
ConnectionStrings__IdentityConnection
ConnectionStrings__HangfireConnection
Jwt__Secret
Email__Password
```

---

## 📝 Adding New Features

### 1. Create Entity (Domain Layer)

```csharp
// src/Core/CleanArchitecture.Domain/Entities/YourEntity.cs
public class YourEntity : BaseAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;

    public static YourEntity Create(string name) =>
        new YourEntity { Name = name };
}
```

### 2. Add Command & Handler (Application Layer)

```csharp
// Command
public record CreateYourEntityCommand(string Name) : IRequest<Result<Guid>>;

// Handler
public class CreateYourEntityCommandHandler
    : IRequestHandler<CreateYourEntityCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateYourEntityCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<Guid>> Handle(CreateYourEntityCommand request, CancellationToken ct)
    {
        var entity = YourEntity.Create(request.Name);
        _context.YourEntities.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(entity.Id);
    }
}
```

### 3. Configure Mapping (Application Layer)

Mapster is configured to auto-scan for `IRegister` implementations — simply create the class and it will be picked up automatically.

```csharp
// src/Core/CleanArchitecture.Application/Features/YourFeature/Mappings/YourEntityMapping.cs
public class YourEntityMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<YourEntity, YourEntityDto>()
            .Map(dest => dest.CustomProperty, src => src.Calculation())
            .IgnoreNullValues(true);
    }
}
```

### 4. Create Controller (Presentation Layer)

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class YourController : ControllerBase
{
    private readonly IMediator _mediator;

    public YourController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourEntityCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
```

---

## 🔒 Security Checklist (Before Production)

> **⚠️ Do not deploy with default development settings.**

- [ ] **Change JWT Secret** — Use a cryptographically strong secret (min 32 characters)
- [ ] **Update Connection Strings** — Use secure, dedicated production database credentials
- [ ] **Enable HTTPS** — Ensure TLS is properly configured and enforce HTTPS redirection
- [ ] **Restrict CORS Origins** — Allow only known, trusted origins
- [ ] **Review Password Policies** — Customize in `Infrastructure.Identity/DependencyInjection.cs`
- [ ] **Secure Hangfire Dashboard** — Restrict access to authenticated admin users only
- [ ] **Rotate Secrets Regularly** — Implement a secrets management strategy (e.g., Azure Key Vault)

---

## 📚 Architecture Overview

| Layer | Project Suffix | Responsibility |
|-------|---------------|----------------|
| **Domain** | `*.Domain` | Entities, value objects, domain events, business rules |
| **Application** | `*.Application` | Use cases, CQRS handlers, validators, interfaces |
| **Persistence** | `*.Infrastructure.Persistence` | EF Core, DbContext, migrations, repositories |
| **Identity** | `*.Infrastructure.Identity` | ASP.NET Core Identity, JWT token service |
| **Shared** | `*.Infrastructure.Shared` | Cross-cutting services (email, datetime, etc.) |
| **WebAPI** | `*.WebAPI` | REST controllers, middleware pipeline, DI configuration |
| **WebUI** | `*.WebUI` | Blazor Server interactive frontend |

---

## 🤝 Contributing

Contributions are very welcome! Please follow the steps below:

1. **Fork** the repository
2. **Create** your feature branch: `git checkout -b feature/AmazingFeature`
3. **Commit** your changes: `git commit -m 'feat: add amazing feature'`
4. **Push** to the branch: `git push origin feature/AmazingFeature`
5. **Open** a Pull Request on [GitHub](https://github.com/MinimDev/dotnet-clean-architecture-template)

---

## 📄 License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgments

- [Clean Architecture](https://8thlight.com/blog/uncle-bob/2012/08/13/the-clean-architecture.html) by **Robert C. Martin (Uncle Bob)**
- [Ardalis CleanArchitecture](https://github.com/ardalis/CleanArchitecture) by **Steve Smith**
- [Jason Taylor's CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture) by **Jason Taylor**

---

<div align="center">
  <strong>Built with ❤️ by <a href="https://github.com/MinimDev">MinimDev</a></strong>
</div>
