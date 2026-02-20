# Clean Architecture Template Solution (.NET 10)

A comprehensive, production-ready ASP.NET Core starter template implementing Clean Architecture principles with CQRS, MediatR, JWT Authentication, and modern API documentation.

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/Minimdev.CleanArchitecture.Template.svg)](https://www.nuget.org/packages/Minimdev.CleanArchitecture.Template)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Minimdev.CleanArchitecture.Template.svg)](https://www.nuget.org/packages/Minimdev.CleanArchitecture.Template)
[![GitHub Stars](https://img.shields.io/github/stars/MinimDev/dotnet-clean-architecture-template.svg)](https://github.com/MinimDev/dotnet-clean-architecture-template/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/MinimDev/dotnet-clean-architecture-template.svg)](https://github.com/MinimDev/dotnet-clean-architecture-template/network)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## 🏗️ Project Structure

```
YourProject/
├── src/
│   ├── Core/
│   │   ├── YourProject.Domain/               # Enterprise business rules
│   │   └── YourProject.Application/          # Application business rules
│   ├── Infrastructure/
│   │   ├── YourProject.Infrastructure.Persistence/   # Data access (EF Core)
│   │   ├── YourProject.Infrastructure.Identity/      # Authentication & JWT
│   │   └── YourProject.Infrastructure.Shared/        # Shared services
│   └── Presentation/
│       ├── YourProject.WebAPI/               # REST API
│       └── YourProject.WebUI/                # Blazor Server UI
├── tests/
│   ├── YourProject.Domain.UnitTests/
│   ├── YourProject.Application.UnitTests/
│   └── YourProject.Application.IntegrationTests/
├── docker-compose.yml
├── Dockerfile
├── YourProject.sln
└── README.md
```

## ✨ Features

### Architecture & Patterns
- ✅ **Clean Architecture** - Separation of concerns with clear layer boundaries
- ✅ **CQRS** - Command Query Responsibility Segregation pattern
- ✅ **Domain-Driven Design** - Rich domain models with business logic
- ✅ **Repository Pattern** - Data access abstraction
- ✅ **Result Pattern** - Consistent error handling

### Technology Stack
- ✅ **.NET 10** - Latest .NET framework
- ✅ **MediatR v12.x** - CQRS implementation
- ✅ **Entity Framework Core 10** - ORM with Code-First approach
- ✅ **SQL Server** - Database (separate DBs for App and Identity)
- ✅ **ASP.NET Core Identity** - User management
- ✅ **JWT Bearer Authentication** - Secure API authentication
- ✅ **OpenTelemetry** - Observability and distributed tracing
- ✅ **AutoMapper v12.0.1** - Object-to-object mapping
- ✅ **FluentValidation** - Request validation
- ✅ **Scalar UI** - Modern API documentation with JWT support
- ✅ **Serilog** - Structured logging (Console + File)
- ✅ **xUnit** - Unit testing framework

### Key Capabilities
- ✅ **API Versioning** - Versioned endpoints (`/api/v1/...`) via URL segment & Header
- ✅ **Rate Limiting** - Built-in ASP.NET Core rate limiting (Global & Auth-specific limits)
- ✅ **Response Caching** - Output caching configured for GET endpoints
- ✅ **Background Jobs** - Hangfire integration with SQL Server storage & `/hangfire` dashboard
- ✅ **Resilience (Polly)** - Standard & Lightweight retry/circuit breaker policies via `Microsoft.Extensions.Http.Resilience`
- ✅ **Email Service** - `IEmailService` abstraction with built-in `SmtpEmailService` implementation
- ✅ **Audit Trails** - Automatic tracking of Created/Modified timestamps
- ✅ **Soft Delete** - Global query filters for deleted entities
- ✅ **Health Checks** - Database connectivity monitoring
- ✅ **Global Exception Handling** - Centralized error management
- ✅ **CORS Support** - Configurable cross-origin requests
- ✅ **Docker Support** - Containerization ready
- ✅ **Visual Studio 2022** - Full IDE support via New Project dialog
- ✅ **.editorconfig** - Comprehensive code style consistency rules

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

**What happens automatically:**
- `CleanArchitecture.Domain` → `MyProject.Domain`
- `namespace CleanArchitecture.Domain` → `namespace MyProject.Domain`
- `src/Core/CleanArchitecture.Domain` → `src/Core/MyProject.Domain`

#### Via Visual Studio 2022

1. Open Visual Studio 2022
2. Click **Create a new project**
3. Search for **"Clean Architecture Solution"**
4. Enter your project name → **Create**

> The template automatically renames all projects and namespaces to match your project name.

### Uninstall

```bash
dotnet new uninstall Minimdev.CleanArchitecture.Template
```

## 🚀 Quick Start

### A. Start from Generated Project (Template User)

After creating your project from the template (`dotnet new cleanarch -n MyProject`), replace `MyProject` with your actual project name in the steps below.

#### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code

#### Setup

1. **Update connection strings**

   Edit `src/Presentation/MyProject.WebAPI/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=MyProjectDB;Trusted_Connection=True;",
       "IdentityConnection": "Server=localhost;Database=MyProjectIdentityDB;Trusted_Connection=True;"
     }
   }
   ```

2. **Apply database migrations**
   ```bash
   # Application DB
   dotnet ef database update --project "src/Infrastructure/MyProject.Infrastructure.Persistence" --startup-project "src/Presentation/MyProject.WebAPI" --context ApplicationDbContext

   # Identity DB
   dotnet ef database update --project "src/Infrastructure/MyProject.Infrastructure.Identity" --startup-project "src/Presentation/MyProject.WebAPI" --context IdentityDbContext
   ```

3. **Run the application**
   ```bash
   dotnet run --project "src/Presentation/MyProject.WebAPI"
   ```

4. **Access the API**
   - **Scalar UI**: `https://localhost:7253/scalar/v1`
   - **Health Check**: `https://localhost:7253/health`

---

### B. Run the Template Source (Contributor)

> This section is for contributors who want to develop or modify the template itself.

1. **Clone the repository**
   ```bash
   git clone https://github.com/MinimDev/dotnet-clean-architecture-template.git
   cd dotnet-clean-architecture-template
   ```

2. **Update connection strings** in `src/Presentation/CleanArchitecture.WebAPI/appsettings.Development.json`

3. **Apply migrations**
   ```bash
   dotnet ef database update --project "src/Infrastructure/CleanArchitecture.Infrastructure.Persistence" --startup-project "src/Presentation/CleanArchitecture.WebAPI" --context ApplicationDbContext

   dotnet ef database update --project "src/Infrastructure/CleanArchitecture.Infrastructure.Identity" --startup-project "src/Presentation/CleanArchitecture.WebAPI" --context IdentityDbContext
   ```

4. **Run**
   ```bash
   dotnet run --project "src/Presentation/CleanArchitecture.WebAPI"
   ```

## 📖 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/Auth/register` | Register a new user |
| POST | `/api/v1/Auth/login` | Login and get JWT token (Rate limited: 10/min) |

### Products *(Requires Authentication)*
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Products` | Get paginated list (Output Cached: 30s) |
| GET | `/api/v1/Products/{id}` | Get by ID (Output Cached: 60s) |
| POST | `/api/v1/Products` | Create product |
| PUT | `/api/v1/Products/{id}` | Update product |
| DELETE | `/api/v1/Products/{id}` | Delete product |

### Background Jobs *(Requires Authentication)*
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/Jobs/fire-and-forget` | Demonstrates immediate background task |
| POST | `/api/v1/Jobs/delayed` | Demonstrates scheduled background task |
| POST | `/api/v1/Jobs/recurring` | Demonstrates CRON-based recurring task |
| POST | `/api/v1/Jobs/recurring/trigger` | Triggers the daily recurring task immediately |

**Hangfire Dashboard:** Available at `https://localhost:7253/hangfire`

### Using JWT in Scalar UI
1. Call `POST /api/Auth/login` to get the token
2. In Scalar UI, click **"Auth Type"** → select **"Bearer"**
3. Paste your JWT token (without "Bearer" prefix)

## 🐳 Docker Support

```bash
# Build and run (includes SQL Server)
docker-compose up -d

# Stop containers
docker-compose down
```

The API will be available at `http://localhost:8080`

## 🧪 Testing

```bash
dotnet test
```

## 🔧 Configuration

Key sections in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanArchitectureDb;Trusted_Connection=True;",
    "IdentityConnection": "Server=localhost;Database=CleanArchitectureIdentityDb;Trusted_Connection=True;",
    "HangfireConnection": "Server=localhost;Database=CleanArchitectureHangfireDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "your-secret-key-min-32-characters",
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

**Environment variables for production/overrides:**
- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__IdentityConnection`
- `ConnectionStrings__HangfireConnection`
- `Jwt__Secret`
- `Email__Password`

## 📝 Adding New Features

### 1. Create Entity (Domain Layer)

```csharp
// src/Core/CleanArchitecture.Domain/Entities/YourEntity.cs
public class YourEntity : BaseAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; }

    public static YourEntity Create(string name) =>
        new YourEntity { Name = name };
}
```

### 2. Add Command/Query (Application Layer)

```csharp
// Command
public record CreateYourEntityCommand(string Name) : IRequest<Result<Guid>>;

// Handler
public class CreateYourEntityCommandHandler
    : IRequestHandler<CreateYourEntityCommand, Result<Guid>>
{
    // Implementation
}
```

### 3. Create Controller (Presentation Layer)

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class YourController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourEntityCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
```

## 🔒 Security Notes

> **⚠️ Before deploying to production:**

1. **Change JWT Secret** — Use a strong, unique secret (min 32 characters)
2. **Update Connection Strings** — Use secure, production database credentials
3. **Enable HTTPS** — Ensure SSL/TLS is properly configured
4. **Configure CORS** — Restrict to known origins only
5. **Review Password Policies** — Adjust in `Infrastructure.Identity/DependencyInjection.cs`
6. **Enable Rate Limiting** — Add rate limiting middleware for production

## 📚 Architecture Overview

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `*.Domain` | Entities, business rules, domain events |
| **Application** | `*.Application` | Use cases, CQRS, validators, interfaces |
| **Persistence** | `*.Infrastructure.Persistence` | EF Core, DbContext, migrations |
| **Identity** | `*.Infrastructure.Identity` | ASP.NET Identity, JWT token service |
| **Shared** | `*.Infrastructure.Shared` | Common services (DateTime, email, etc.) |
| **WebAPI** | `*.WebAPI` | REST controllers, middleware, DI config |
| **WebUI** | `*.WebUI` | Blazor Server frontend |

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request on [GitHub](https://github.com/MinimDev/dotnet-clean-architecture-template).

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- [Clean Architecture](https://8thlight.com/blog/uncle-bob/2012/08/13/the-clean-architecture.html) by Robert C. Martin
- [Ardalis CleanArchitecture](https://github.com/ardalis/CleanArchitecture)
- [Jason Taylor's CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)

---

**Built with ❤️ by [Minimdev](https://github.com/MinimDev)**
