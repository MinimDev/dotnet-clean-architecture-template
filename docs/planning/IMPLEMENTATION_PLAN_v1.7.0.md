# 🚀 Implementation Plan — Clean Architecture Template v1.7.0

> **Target Version:** 1.7.0  
> **Total Estimate:** 5 working sessions  
> **Created Date:** 2026-10-19  
> **Philosophy:** Every feature adheres strictly to Clean Architecture — abstraction in the Application layer, implementation in the Infrastructure layer, presentation in WebUI.

---

## 📊 Overall Status

| Phase | Feature | Status | Branch |
|---|---|---|---|
| 1 | Idempotency Key + Double-Submit Fix | ✅ Completed | `feat/idempotency-key` |
| 2 | Audit Trail Table | ✅ Completed | `feat/audit-trail` |
| 3 | App Settings / Feature Flags | ✅ Completed | `feat/app-settings` |
| 4 | Notification System (In-App) | ✅ Completed | `feat/notification-system` |
| 5 | Permission-Based Access Control | ✅ Completed | `feat/pbac` |

**Status Legend:** ⬜ Not Started · 🔄 In Progress · ✅ Completed · 🚧 Blocked

---

## 🗺️ Milestone Overview

```
v1.6.1 (Baseline)
    │
    ├── Phase 1: Foundation & Bug Fix
    │   └── [v1.7.0-alpha] Idempotency Key + Double-Submit Fix
    │
    ├── Phase 2: Data Integrity & History
    │   └── [v1.7.0-beta] Audit Trail Table
    │
    ├── Phase 3: Admin Powertools
    │   └── [v1.7.0-rc1] App Settings / Feature Flags Management
    │
    ├── Phase 4: User Experience
    │   └── [v1.7.0-rc2] Notification System (In-App)
    │
    └── Phase 5: Security Upgrade
        └── [v1.7.0] Permission-Based Access Control (PBAC)
```

---

## Phase 1 — Idempotency Key + UI Double-Submit Fix
**Target Version:** `v1.7.0-alpha`  
**Estimate:** 1 session  
**Branch:** `feat/idempotency-key`  
**Status:** ✅ Completed

### 🎯 Objective
Prevent duplicate data caused by rapid double-clicks, network retries, or repeated API calls.

### 🐛 Analyzed Root Causes
1. **Frontend:** `_isSaving = true` was previously set AFTER `await form.Validate()`, creating an async window where the submit button remained clickable.
2. **Backend:** No unique constraint on product name in the database; IDs are generated via `Guid.NewGuid()`, meaning two identical requests produced two separate records.
3. **Domain:** No duplicate validation inside `CreateProductCommandHandler`.
4. **Cascading Effects:** 2 entities → 2 domain events → 2 MassTransit integration messages.

### ✅ Checklist

#### A. Frontend Fix (Blazor WebUI)
- [x] **MODIFY** `src/Presentation/CleanArchitecture.WebUI/Components/Pages/Products/CreateProductDialog.razor`
  - Added `if (_isSaving) return;` as the first guard in `Submit()`
  - Moved `_isSaving = true; StateHasChanged();` to BEFORE `await form.Validate()`
- [x] Applied identical guard pattern across all Edit and Delete Confirm dialogs.

#### B. Application Layer — Abstraction
- [x] **CREATE** `src/Core/CleanArchitecture.Application/Common/Interfaces/IIdempotencyService.cs`
  ```csharp
  public interface IIdempotencyService
  {
      Task<IdempotencyResult?> GetCachedResponseAsync(string idempotencyKey);
      Task CacheResponseAsync(string idempotencyKey, object response, TimeSpan ttl);
  }
  
  public record IdempotencyResult(int StatusCode, string Body);
  ```
- [x] **CREATE** `src/Core/CleanArchitecture.Application/Common/Behaviours/IdempotencyBehaviour.cs`
  - MediatR Pipeline Behaviour intercepting all commands (not queries)
  - Reads `Idempotency-Key` header from `IHttpContextAccessor`
  - If key exists in cache → returns cached response immediately (idempotent)
  - If not → processes pipeline normally, then caches output response

#### C. Infrastructure Layer — Implementation
- [x] **CREATE** `src/Infrastructure/CleanArchitecture.Infrastructure.Shared/Services/IdempotencyService.cs`
  - Default implementation backed by `IMemoryCache` (no external dependency)
  - Default TTL: 24 hours (configurable)
  - Cache key format: `idempotency:{idempotency-key-value}`

#### D. Persistence & Config
- [x] **MODIFY** `appsettings.json` — added `"Idempotency": { "CacheDurationHours": 24 }` section
- [x] **MODIFY** `DependencyInjection.cs` in Shared — registered `IIdempotencyService`
- [x] **MODIFY** `DependencyInjection.cs` in Application — registered `IdempotencyBehaviour` in pipeline

#### E. Documentation
- [x] Added Idempotency documentation in Scalar UI (custom operation filter)
- [x] Updated `README.md` — added Idempotency Key usage section

---

## Phase 2 — Audit Trail Table
**Target Version:** `v1.7.0-beta`  
**Estimate:** 1.5 sessions  
**Branch:** `feat/audit-trail`  
**Status:** ✅ Completed

### 🎯 Objective
Persist comprehensive mutation history for all data entities (Insert/Update/Delete) into a dedicated audit table automatically without modifying individual handlers.

### 🔧 Approach
Extended existing `AuditableEntityInterceptor` to capture mutation diffs into the `AuditLogs` table within the same database transaction.

### ✅ Checklist

#### A. Domain Layer
- [x] **CREATE** `src/Core/CleanArchitecture.Domain/Entities/AuditLog.cs`
  ```csharp
  public class AuditLog
  {
      public Guid Id { get; set; }
      public string EntityName { get; set; }
      public string Action { get; set; }      // Created | Updated | Deleted
      public string PrimaryKey { get; set; }
      public string? OldValues { get; set; }  // JSON serialize
      public string? NewValues { get; set; }  // JSON serialize
      public string[]? AffectedColumns { get; set; }
      public string? UserId { get; set; }
      public string? UserName { get; set; }
      public DateTime Timestamp { get; set; }
  }
  ```

#### B. Application Layer
- [x] **MODIFY** `IApplicationDbContext.cs` — added `DbSet<AuditLog> AuditLogs`
- [x] **CREATE** `Features/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQuery.cs`
  - Parameters: `EntityName?`, `Action?`, `UserId?`, `DateFrom?`, `DateTo?`, `PageNumber`, `PageSize`
- [x] **CREATE** `Features/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQueryHandler.cs`
  - Reuses `PaginatedList<T>`

#### C. Infrastructure Layer
- [x] **MODIFY** `Interceptors/AuditableEntityInterceptor.cs`
  - Captures `EntityState.Added` → OldValues=null, NewValues=current
  - Captures `EntityState.Modified` → OldValues=original, NewValues=current, AffectedColumns
  - Captures `ISoftDeletable` entity soft-delete → Action="Deleted"
  - Serialized via `System.Text.Json.JsonSerializer`
- [x] **CREATE** `Configurations/AuditLogConfiguration.cs`
  - Indexes on `EntityName`, `Action`, `Timestamp`, `UserId`
  - HasQueryFilter: none (audit logs cannot be soft deleted)
- [x] **CREATE** Migration: `AddAuditLogs`

#### D. WebAPI
- [x] **CREATE** `Controllers/AuditLogsController.cs`
  - `GET /api/v1/audit-logs` — with query params filters + pagination
  - Role: `Admin` only

#### E. WebUI — Audit Log Page (Premium Design)
- [x] **CREATE** `Components/Pages/AuditLogs/AuditLogs.razor`
  - Timeline-style layout (not a plain table)
  - Color coding: 🟢 Created, 🟡 Updated, 🔴 Deleted
  - Collapsible diff view: OldValues vs NewValues side-by-side
  - Filter panel: EntityName dropdown, Action chips, DateRange picker
  - Active filter chips clearable individually
  - Skeleton loading during data fetching
  - Empty state with "No audit logs found" illustration
- [x] **MODIFY** `NavMenu.razor` — added "Audit Logs" sidebar link (Admin only)

---

## Phase 3 — App Settings / Feature Flags Management
**Target Version:** `v1.7.0-rc1`  
**Estimate:** 1 session  
**Branch:** `feat/app-settings`  
**Status:** ✅ Completed

### 🎯 Objective
Enable administrators to modify runtime application configuration and feature flags through the UI without requiring redeployment.

### ✅ Checklist

#### A. Domain Layer
- [x] **CREATE** `Domain/Entities/AppSetting.cs`
  ```csharp
  public class AppSetting : BaseAuditableEntity
  {
      public string Key { get; private set; }      // e.g. "Features.ProductExport.Enabled"
      public string Value { get; private set; }    // e.g. "true", "50", "smtp.example.com"
      public string DataType { get; private set; } // "string" | "boolean" | "number"
      public string? Description { get; private set; }
      public string Group { get; private set; }    // "General" | "Email" | "Features" | "Security"
      public bool IsReadOnly { get; private set; } // locks setting from UI modification
  }
  ```
- [x] **CREATE** `Domain/Events/AppSettingChangedEvent.cs`

#### B. Application Layer
- [x] **CREATE** `Common/Interfaces/IAppSettingsService.cs`
  ```csharp
  public interface IAppSettingsService
  {
      Task<string?> GetAsync(string key, string? defaultValue = null);
      Task<T?> GetAsync<T>(string key, T? defaultValue = default);
      Task SetAsync(string key, string value, CancellationToken ct = default);
      Task<bool> IsFeatureEnabledAsync(string featureKey);
      Task<IReadOnlyList<AppSettingDto>> GetAllAsync(string? group = null);
  }
  ```
- [x] **CREATE** `Features/AppSettings/` — Commands: `UpdateAppSetting`; Queries: `GetAppSettings`, `GetAppSettingByKey`

#### C. Infrastructure Layer
- [x] **CREATE** `Services/DbAppSettingsService.cs`
  - Reads from DB, cached in-memory via `IMemoryCache`
  - Automatically invalidates cache when `SetAsync` is invoked
  - Fallback to `IConfiguration` (appsettings.json) if key is not found in database

#### D. Persistence
- [x] **CREATE** `Configurations/AppSettingConfiguration.cs` — Unique index on `Key`
- [x] **CREATE** Migration: `AddAppSettings`
- [x] **CREATE** Seeder: default configuration entries (General, Email, Features)

#### E. WebUI — Settings Admin Panel (Premium Design)
- [x] **CREATE** `Components/Pages/Settings/AppSettings.razor`
  - Card view per group (accordion / tab navigation)
  - Inline edit: click value → input field renders directly
  - Toggle switch for DataType="boolean"
  - Numeric input for DataType="number"
  - Text input for DataType="string"
  - Lock icon for IsReadOnly settings
  - Save confirmation via Snackbar
  - Search/filter settings by key or description
- [x] **MODIFY** `NavMenu.razor` — added "App Settings" sidebar link (Admin only)

---

## Phase 4 — Notification System (In-App)
**Target Version:** `v1.7.0-rc2`  
**Estimate:** 1.5 sessions  
**Branch:** `feat/notification-system`  
**Status:** ✅ Completed

### 🎯 Objective
Deliver in-app notifications publishable from any component (Hangfire, MassTransit Consumers, CQRS Handlers) and presented via the WebUI bell icon with real-time-like polling.

### ✅ Checklist

#### A. Domain Layer
- [x] **CREATE** `Domain/Entities/Notification.cs`
  ```csharp
  public class Notification : BaseEntity
  {
      public string? UserId { get; private set; }  // null = broadcast to all users
      public string Title { get; private set; }
      public string Message { get; private set; }
      public NotificationType Type { get; private set; } // Info | Success | Warning | Error
      public bool IsRead { get; private set; }
      public string? ActionUrl { get; private set; }
      public DateTime CreatedAt { get; private set; }
      public DateTime? ReadAt { get; private set; }
  }
  ```
- [x] **CREATE** `Domain/Enums/NotificationType.cs`

#### B. Application Layer
- [x] **CREATE** `Common/Interfaces/INotificationService.cs`
  ```csharp
  public interface INotificationService
  {
      Task SendAsync(string userId, string title, string message,
                     NotificationType type = NotificationType.Info,
                     string? actionUrl = null,
                     CancellationToken ct = default);
      Task BroadcastAsync(string title, string message,
                          NotificationType type = NotificationType.Info,
                          CancellationToken ct = default);
      Task MarkAsReadAsync(Guid notificationId, string userId, CancellationToken ct = default);
      Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
      Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
  }
  ```
- [x] **CREATE** `Features/Notifications/Queries/GetNotifications/` — paginated listing
- [x] **CREATE** `Features/Notifications/Queries/GetUnreadCount/` — scalar count query
- [x] **CREATE** `Features/Notifications/Commands/MarkAsRead/`
- [x] **CREATE** `Features/Notifications/Commands/MarkAllAsRead/`

#### C. Infrastructure Layer
- [x] **CREATE** `Services/NotificationService.cs` in Shared
- [x] **CREATE** `Configurations/NotificationConfiguration.cs`
- [x] **CREATE** Migration: `AddNotifications`

#### D. Integration — Demo Notifications
- [x] **MODIFY** `ExampleJob.cs` — sends notification to all Admins upon job completion
- [x] **MODIFY** `ProductCreatedConsumer.cs` — broadcasts notification when new product is created

#### E. WebAPI
- [x] **CREATE** `Controllers/NotificationsController.cs`
  - `GET /api/v1/notifications` — list with pagination
  - `GET /api/v1/notifications/unread-count`
  - `PATCH /api/v1/notifications/{id}/read`
  - `PATCH /api/v1/notifications/read-all`

#### F. WebUI — Bell Icon & Notification Panel (Premium Design)
- [x] **MODIFY** `MainLayout.razor`
  - Added bell icon in topbar right actions
  - Badge with unread count (hidden if 0)
  - Click → toggles NotificationPanel dropdown
- [x] **CREATE** `Components/Shared/NotificationPanel.razor`
  - Animated slide-down panel (glassmorphism style)
  - Header: "Notifications" + "Mark all as read" button
  - Item: type icon, bold title, truncated message, relative timestamp
  - Unread items: highlighted background
  - Click item → mark as read + navigate to ActionUrl if present
  - Empty state: "You're all caught up! 🎉"
  - Footer: "View all notifications" link
- [x] Auto-refresh unread count every 30 seconds (polling)

---

## Phase 5 — Permission-Based Access Control (PBAC)
**Target Version:** `v1.7.0`  
**Estimate:** 2 sessions  
**Branch:** `feat/pbac`  
**Status:** ✅ Completed  
**Dependency:** Phase 3 (App Settings) completed

### 🎯 Objective
Replace static `[Authorize(Roles = "Admin")]` role checks with granular feature permissions configurable dynamically through the UI.

### ✅ Checklist

#### A. Application Layer — Permission Constants
- [x] **CREATE** `Common/Security/Permissions.cs`
  ```csharp
  public static class Permissions
  {
      public static class Products
      {
          public const string View   = "Permissions.Products.View";
          public const string Create = "Permissions.Products.Create";
          public const string Edit   = "Permissions.Products.Edit";
          public const string Delete = "Permissions.Products.Delete";
      }
      public static class Users
      {
          public const string View        = "Permissions.Users.View";
          public const string ManageRoles = "Permissions.Users.ManageRoles";
          public const string ManagePermissions = "Permissions.Users.ManagePermissions";
      }
      public static class AuditLogs
      {
          public const string View = "Permissions.AuditLogs.View";
      }
      public static class AppSettings
      {
          public const string View = "Permissions.AppSettings.View";
          public const string Edit = "Permissions.AppSettings.Edit";
      }
      public static class Notifications
      {
          public const string View   = "Permissions.Notifications.View";
          public const string Manage = "Permissions.Notifications.Manage";
      }
  }
  ```
- [x] **CREATE** `Common/Security/HasPermissionAttribute.cs`
  ```csharp
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
  public class HasPermissionAttribute : AuthorizeAttribute
  {
      public HasPermissionAttribute(string permission) : base(permission) { }
  }
  ```
- [x] **CREATE** `Common/Security/PermissionAuthorizationHandler.cs`
- [x] **CREATE** `Common/Security/PermissionPolicyProvider.cs`
- [x] **MODIFY** `DependencyInjection.cs` — registered `IAuthorizationHandler`, `IAuthorizationPolicyProvider`, and `IPermissionService`

#### B. Identity Layer — Role-Permission Mapping
- [x] **CREATE** `Models/RolePermission.cs`
  ```csharp
  public class RolePermission
  {
      public int Id { get; set; }
      public string RoleName { get; set; }
      public string Permission { get; set; }  // value from Permissions.* constants
  }
  ```
- [x] **CREATE** `Services/PermissionService.cs`
  - `GetPermissionsForRoleAsync(string roleName)` → List<string> permission strings
  - Claims-based: permissions injected as claims into JWT during login/refresh
  - Cached per roleName with `IMemoryCache` and auto-invalidated on permission updates
- [x] **MODIFY** `TokenService.cs` — appends permission claims (`"permission"`) into JWT

#### C. Persistence (IdentityDB)
- [x] **CREATE** Migration: `AddRolePermissions` in Identity project
- [x] **CREATE** Default seeder:
  - Role `Admin` → all permissions
  - Role `Member` → `Products.View` only

#### D. WebAPI — Update All Controllers
- [x] **MODIFY** `ProductsController.cs` — migrated to `[HasPermission(Permissions.Products.*)]`
- [x] **MODIFY** `UsersController.cs` — migrated to `[HasPermission(Permissions.Users.*)]` and added PBAC endpoints
- [x] **MODIFY** `AuditLogsController.cs` — `[HasPermission(Permissions.AuditLogs.View)]`
- [x] **MODIFY** `AppSettingsController.cs` — `[HasPermission(Permissions.AppSettings.*)]`

#### E. WebUI — User Management Enhancement
- [x] **MODIFY** `Users.razor`
  - "Users List" tab & "Role Permissions (PBAC)" tab
  - Checkbox tree grouped by module category (`Products`, `Users`, etc.)
  - Save/Reset per role with instant snackbar feedback
- [x] Sidebar: dynamic menu filtering based on user permissions via `TokenProvider.HasPermission(...)`

---

## 🎨 UI/UX Overhaul — Parallel Execution

> Every phase introduced visual refinements, engineered concurrently with core features.

### Updated Components

| Phase | Component | UI Polish |
|---|---|---|
| 1 | All Dialogs | Double-submit guard, smoother loading states |
| 2 | Sidebar + AuditLogs.razor | Timeline layout, side-by-side diff view, color coding |
| 3 | Settings.razor | Card layout per group, inline editing, toggle switch |
| 4 | MainLayout + NotificationPanel | Bell icon badge, animated dropdown, glassmorphism |
| 5 | Users.razor | Permission checkbox matrix, visual dirty indicators |

---

## ✅ Definition of Done (Per Phase)

- [x] Unit tests for Application layer (new command/query handlers)
- [x] Integration tests in `CleanArchitecture.Application.IntegrationTests`
- [x] XML doc `<summary>` across public interfaces and methods
- [x] `appsettings.json` updated with new configuration sections
- [x] `README.md` updated with new feature descriptions
- [x] `CHANGELOG.md` updated with release entries

---

## 📝 Architectural Decisions & Development Notes

### Phase 1
- **Clean Architecture Boundary**: `CleanArchitecture.Application` must never reference ASP.NET Core `HttpContext`. The `IIdempotencyKeyAccessor` abstraction ensures MediatR pipeline isolation and portability.
- **DI Registration**: Used `TryAddTransient` for the fallback `NullIdempotencyKeyAccessor` registration so the Presentation layer can override it cleanly.
- **EF Core Multi-Provider**: Swapping from SQL Server to SQLite in `CustomWebApplicationFactory` requires clearing internal EF Core service descriptors to avoid provider collision.

### Phase 2
- **Atomic Change Auditing**: `AuditableEntityInterceptor` runs before `base.SavingChangesAsync`, ensuring `AuditLog` records are persisted in the exact same database transaction as mutated entities.
- **Re-entrancy Protection**: The interceptor explicitly skips `AuditLog` entities and owned types to avoid infinite auditing loops.
- **Cross-Database Value Conversion**: `AffectedColumns` is stored with a JSON ValueConverter in `AuditLogConfiguration`, providing complete compatibility between SQL Server in production and SQLite in test environments.
- **Integration Test Coverage**: `CustomWebApplicationFactory` registers `AuditableEntityInterceptor` on SQLite options so mutation auditing (Insert, Update, Soft-delete) is verified end-to-end.

---

*Plan created on: 2026-10-19*  
*Plan version: 1.0*
