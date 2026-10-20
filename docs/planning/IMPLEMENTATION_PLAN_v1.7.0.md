# 🚀 Implementation Plan — Clean Architecture Template v1.7.0

> **Versi Target:** 1.7.0  
> **Estimasi Total:** 5 sesi kerja  
> **Tanggal Dibuat:** 2026-10-19  
> **Filosofi:** Setiap fitur mengikuti Clean Architecture — abstraction di Application layer, implementasi di Infrastructure layer, presentasi di WebUI.

---

## 📊 Status Keseluruhan

| Phase | Fitur | Status | Branch |
|---|---|---|---|
| 1 | Idempotency Key + Double-Submit Fix | ✅ Selesai | `feat/idempotency-key` |
| 2 | Audit Trail Table | ✅ Selesai | `feat/audit-trail` |
| 3 | App Settings / Feature Flags | ⬜ Belum Mulai | `feat/app-settings` |
| 4 | Notification System (In-App) | ⬜ Belum Mulai | `feat/notification-system` |
| 5 | Permission-Based Access Control | ⬜ Belum Mulai | `feat/pbac` |

**Status Legend:** ⬜ Belum Mulai · 🔄 Sedang Dikerjakan · ✅ Selesai · 🚧 Blocked

---

## 🗺️ Overview Milestone

```
v1.6.1 (saat ini)
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
**Target Versi:** `v1.7.0-alpha`  
**Estimasi:** 1 sesi  
**Branch:** `feat/idempotency-key`  
**Status:** ✅ Selesai

### 🎯 Tujuan
Mencegah data duplikat akibat double-click, network retry, atau repeated API call.

### 🐛 Root Cause yang Sudah Dianalisis
1. **Frontend:** `_isSaving = true` di-set SETELAH `await form.Validate()` — ada jeda async dimana tombol belum disabled
2. **Backend:** Tidak ada unique constraint di DB untuk nama produk; ID selalu `Guid.NewGuid()` → dua request identik = 2 record berbeda
3. **Domain:** Tidak ada pengecekan duplikasi di `CreateProductCommandHandler`
4. **Efek berantai:** 2 entity → 2 domain events → 2 integration messages MassTransit

### ✅ Checklist

#### A. Frontend Fix (Blazor WebUI)
- [ ] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Pages/Products/CreateProductDialog.razor`
  - Tambah `if (_isSaving) return;` sebagai baris pertama di method `Submit()`
  - Pindahkan `_isSaving = true; StateHasChanged();` ke SEBELUM `await form.Validate()`
- [ ] Terapkan pola guard yang sama ke semua dialog Edit dan Delete Confirm

#### B. Application Layer — Abstraction
- [ ] **BUAT** `src/Core/CleanArchitecture.Application/Common/Interfaces/IIdempotencyService.cs`
  ```csharp
  public interface IIdempotencyService
  {
      Task<IdempotencyResult?> GetCachedResponseAsync(string idempotencyKey);
      Task CacheResponseAsync(string idempotencyKey, object response, TimeSpan ttl);
  }
  
  public record IdempotencyResult(int StatusCode, string Body);
  ```
- [ ] **BUAT** `src/Core/CleanArchitecture.Application/Common/Behaviours/IdempotencyBehaviour.cs`
  - MediatR Pipeline Behaviour yang intercept semua ICommand (bukan query)
  - Baca header `Idempotency-Key` dari `IHttpContextAccessor`
  - Jika key sudah ada di cache → return cached response langsung (idempotent)
  - Jika belum → proses normal, lalu cache hasilnya

#### C. Infrastructure Layer — Implementation
- [ ] **BUAT** `src/Infrastructure/CleanArchitecture.Infrastructure.Shared/Services/IdempotencyService.cs`
  - Implementasi menggunakan `IMemoryCache` (default, no external dependency)
  - TTL default: 24 jam (configurable)
  - Format cache key: `idempotency:{idempotency-key-value}`

#### D. Persistence & Config
- [ ] **UBAH** `appsettings.json` — tambah section `"Idempotency": { "CacheDurationHours": 24 }`
- [ ] **UBAH** `DependencyInjection.cs` di Shared — register `IIdempotencyService`
- [ ] **UBAH** `DependencyInjection.cs` di Application — register `IdempotencyBehaviour` ke pipeline

#### E. Documentation
- [ ] Tambah penjelasan Idempotency di Scalar UI (custom operation filter)
- [ ] Update `README.md` — tambah section Idempotency Key usage

---

## Phase 2 — Audit Trail Table
**Target Versi:** `v1.7.0-beta`  
**Estimasi:** 1.5 sesi  
**Branch:** `feat/audit-trail`  
**Status:** ✅ Selesai

### 🎯 Tujuan
Menyimpan riwayat lengkap setiap perubahan data (Insert/Update/Delete) ke tabel terpisah, otomatis tanpa perubahan di handler.

### 🔧 Pendekatan
Extend `AuditableEntityInterceptor` yang sudah ada untuk capture detail perubahan ke tabel `AuditLogs` dalam transaksi yang sama dengan entity.

### ✅ Checklist

#### A. Domain Layer
- [x] **BUAT** `src/Core/CleanArchitecture.Domain/Entities/AuditLog.cs`
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
- [x] **UBAH** `IApplicationDbContext.cs` — tambah `DbSet<AuditLog> AuditLogs`
- [x] **BUAT** `Features/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQuery.cs`
  - Parameter: `EntityName?`, `Action?`, `UserId?`, `DateFrom?`, `DateTo?`, `PageNumber`, `PageSize`
- [x] **BUAT** `Features/AuditLogs/Queries/GetAuditLogs/GetAuditLogsQueryHandler.cs`
  - Reuse `PaginatedList<T>`

#### C. Infrastructure Layer
- [x] **UBAH** `Interceptors/AuditableEntityInterceptor.cs`
  - Capture `EntityState.Added` → log OldValues=null, NewValues=current
  - Capture `EntityState.Modified` → log OldValues=original, NewValues=current, AffectedColumns
  - Capture `ISoftDeletable` entity soft-delete → log Action="Deleted"
  - Serialize via `System.Text.Json.JsonSerializer`
- [x] **BUAT** `Configurations/AuditLogConfiguration.cs`
  - Index pada `EntityName`, `Action`, `Timestamp`, `UserId`
  - HasQueryFilter: tidak ada (audit log tidak boleh soft-deleted)
- [x] **BUAT** Migration: `AddAuditLogs`

#### D. WebAPI
- [x] **BUAT** `Controllers/AuditLogsController.cs`
  - `GET /api/v1/audit-logs` — dengan query params filter + pagination
  - Role: `Admin` only

#### E. WebUI — Halaman Audit Log (Premium Design)
- [x] **BUAT** `Components/Pages/AuditLogs/AuditLogs.razor`
  - Timeline-style layout (bukan plain table)
  - Color coding: 🟢 Created, 🟡 Updated, 🔴 Deleted
  - Collapsible diff view: OldValues vs NewValues side-by-side
  - Filter panel: EntityName dropdown, Action chips, DateRange picker
  - Active filter chips yang bisa di-clear satu per satu
  - Skeleton loading saat data fetching
  - Empty state dengan ilustrasi "No audit logs found"
- [x] **UBAH** `NavMenu.razor` — tambah link "Audit Logs" di sidebar (Admin only)

---

## Phase 3 — App Settings / Feature Flags Management
**Target Versi:** `v1.7.0-rc1`  
**Estimasi:** 1 sesi  
**Branch:** `feat/app-settings`  
**Status:** ✅ Selesai

### 🎯 Tujuan
Admin bisa mengubah konfigurasi aplikasi dan feature flags melalui UI tanpa perlu redeploy.

### ✅ Checklist

#### A. Domain Layer
- [x] **BUAT** `Domain/Entities/AppSetting.cs`
  ```csharp
  public class AppSetting : BaseAuditableEntity
  {
      public string Key { get; private set; }      // e.g. "Features.ProductExport.Enabled"
      public string Value { get; private set; }    // e.g. "true", "50", "smtp.example.com"
      public string DataType { get; private set; } // "string" | "boolean" | "number"
      public string? Description { get; private set; }
      public string Group { get; private set; }    // "General" | "Email" | "Features" | "Security"
      public bool IsReadOnly { get; private set; } // lock setting dari UI
  }
  ```
- [x] **BUAT** `Domain/Events/AppSettingChangedEvent.cs`

#### B. Application Layer
- [x] **BUAT** `Common/Interfaces/IAppSettingsService.cs`
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
- [x] **BUAT** `Features/AppSettings/` — Commands: `UpdateAppSetting`; Queries: `GetAppSettings`, `GetAppSettingByKey`

#### C. Infrastructure Layer
- [x] **BUAT** `Services/DbAppSettingsService.cs`
  - Read dari DB, cache in-memory via `IMemoryCache`
  - Invalidate cache otomatis saat `SetAsync` dipanggil
  - Fallback ke `IConfiguration` (appsettings.json) jika key tidak ditemukan di DB

#### D. Persistence
- [x] **BUAT** `Configurations/AppSettingConfiguration.cs` — Unique index pada `Key`
- [x] **BUAT** Migration: `AddAppSettings`
- [x] **BUAT** Seeder: default settings awal (General, Email, Features)

#### E. WebUI — Settings Admin Panel (Premium Design)
- [x] **BUAT** `Components/Pages/Settings/AppSettings.razor`
  - Tampilan card per group (accordion / tab navigation)
  - Inline edit: click value → input field muncul langsung (no dialog)
  - Toggle switch untuk DataType="boolean"
  - Numeric input untuk DataType="number"
  - Text input untuk DataType="string"
  - Lock icon untuk IsReadOnly settings
  - Save confirmation dengan Snackbar
  - Search/filter settings by key atau description
- [x] **UBAH** `NavMenu.razor` — tambah link "App Settings" di sidebar (Admin only)

---

## Phase 4 — Notification System (In-App)
**Target Versi:** `v1.7.0-rc2`  
**Estimasi:** 1.5 sesi  
**Branch:** `feat/notification-system`  
**Status:** ⬜ Belum Mulai

### 🎯 Tujuan
Notifikasi in-app yang bisa dipublikasi dari mana saja (Hangfire, Consumer, Handler) dan ditampilkan di bell icon WebUI dengan real-time-like polling.

### ✅ Checklist

#### A. Domain Layer
- [ ] **BUAT** `Domain/Entities/Notification.cs`
  ```csharp
  public class Notification : BaseEntity
  {
      public string? UserId { get; private set; }  // null = broadcast semua user
      public string Title { get; private set; }
      public string Message { get; private set; }
      public NotificationType Type { get; private set; } // Info | Success | Warning | Error
      public bool IsRead { get; private set; }
      public string? ActionUrl { get; private set; }
      public DateTime CreatedAt { get; private set; }
      public DateTime? ReadAt { get; private set; }
  }
  ```
- [ ] **BUAT** `Domain/Enums/NotificationType.cs`

#### B. Application Layer
- [ ] **BUAT** `Common/Interfaces/INotificationService.cs`
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
- [ ] **BUAT** `Features/Notifications/Queries/GetNotifications/` — list dengan pagination
- [ ] **BUAT** `Features/Notifications/Queries/GetUnreadCount/` — simple int query
- [ ] **BUAT** `Features/Notifications/Commands/MarkAsRead/`
- [ ] **BUAT** `Features/Notifications/Commands/MarkAllAsRead/`

#### C. Infrastructure Layer
- [ ] **BUAT** `Services/NotificationService.cs` di Shared
- [ ] **BUAT** `Configurations/NotificationConfiguration.cs`
- [ ] **BUAT** Migration: `AddNotifications`

#### D. Integration — Demo Notifications
- [ ] **UBAH** `ExampleJob.cs` — kirim notifikasi ke semua Admin saat job selesai
- [ ] **UBAH** `ProductCreatedConsumer.cs` — broadcast notifikasi produk baru dibuat

#### E. WebAPI
- [ ] **BUAT** `Controllers/NotificationsController.cs`
  - `GET /api/v1/notifications` — list with pagination
  - `GET /api/v1/notifications/unread-count`
  - `PATCH /api/v1/notifications/{id}/read`
  - `PATCH /api/v1/notifications/read-all`

#### F. WebUI — Bell Icon & Notification Panel (Premium Design)
- [ ] **UBAH** `MainLayout.razor`
  - Tambah bell icon di topbar kanan
  - Badge merah dengan unread count (hidden jika 0)
  - Click → toggle NotificationPanel dropdown
- [ ] **BUAT** `Components/Shared/NotificationPanel.razor`
  - Animated slide-down panel (glassmorphism style)
  - Header: "Notifications" + "Mark all as read" button
  - List item: icon per type, title bold, message truncated, relative time (2 min ago)
  - Unread items: highlighted background
  - Click item → mark as read + navigate ke ActionUrl jika ada
  - Empty state: "You're all caught up! 🎉"
  - Footer: "View all notifications" link
- [ ] Auto-refresh unread count setiap 30 detik (polling)

---

## Phase 5 — Permission-Based Access Control (PBAC)
**Target Versi:** `v1.7.0`  
**Estimasi:** 2 sesi  
**Branch:** `feat/pbac`  
**Status:** ⬜ Belum Mulai  
**Dependency:** Phase 3 (App Settings) harus selesai terlebih dahulu

### 🎯 Tujuan
Mengganti hardcoded `[Authorize(Roles = "Admin")]` dengan permission granular per fitur yang bisa di-manage dari UI.

### ✅ Checklist

#### A. Application Layer — Permission Constants
- [ ] **BUAT** `Common/Security/Permissions.cs`
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
          public const string Manage = "Permissions.Notifications.Manage";
      }
  }
  ```
- [ ] **BUAT** `Common/Security/HasPermissionAttribute.cs`
  ```csharp
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
  public class HasPermissionAttribute : AuthorizeAttribute
  {
      public HasPermissionAttribute(string permission) : base(permission) { }
  }
  ```
- [ ] **BUAT** `Common/Security/PermissionAuthorizationHandler.cs`
- [ ] **UBAH** `DependencyInjection.cs` — register `IAuthorizationHandler`

#### B. Identity Layer — Role-Permission Mapping
- [ ] **BUAT** `Models/RolePermission.cs`
  ```csharp
  public class RolePermission
  {
      public string RoleId { get; set; }
      public string Permission { get; set; }  // nilai dari Permissions.* constants
  }
  ```
- [ ] **BUAT** `Services/PermissionService.cs`
  - `GetPermissionsForUserAsync(string userId)` → List<string> permission strings
  - Claims-based: permissions di-inject sebagai claims ke JWT saat login/refresh
  - Cache per userId dengan invalidation saat permissions berubah
- [ ] **UBAH** `TokenService.cs` — sertakan permission claims di JWT

#### C. Persistence (IdentityDB)
- [ ] **BUAT** `Migration: AddRolePermissions` di Identity project
- [ ] **BUAT** Seeder default:
  - Role `Admin` → semua permissions
  - Role `Member` → `Products.View` saja

#### D. WebAPI — Update All Controllers
- [ ] **UBAH** `ProductsController.cs` — ganti ke `[HasPermission(Permissions.Products.Create)]` dll
- [ ] **UBAH** `UsersController.cs` — ganti ke `[HasPermission(Permissions.Users.View)]`
- [ ] **UBAH** `AuditLogsController.cs` — `[HasPermission(Permissions.AuditLogs.View)]`
- [ ] **UBAH** `JobsController.cs` — tetap Admin-only lewat permission

#### E. WebUI — User Management Enhancement
- [ ] **UBAH** `Users.razor`
  - Tambah tab "Permissions" per role (accordion)
  - Checkbox tree grouped by category (`Products`, `Users`, dll.)
  - Save/Reset per role
  - Visual indicator: active permissions per user di user list
- [ ] Sidebar: hide/show menu items berdasarkan permission user yang sedang login

---

## 🎨 UI/UX Overhaul — Berjalan Paralel

> Setiap phase membawa peningkatan visual, dikerjakan bersamaan dengan fitur utama.

### Komponen yang Diperbarui

| Phase | Komponen | Perbaikan UI |
|---|---|---|
| 1 | Semua Dialog | Double-submit guard, loading state lebih smooth |
| 2 | Sidebar + AuditLogs.razor | Timeline layout, diff view, color coding |
| 3 | Settings.razor | Card layout per group, inline edit, toggle switch |
| 4 | MainLayout + NotificationPanel | Bell icon badge, animated dropdown, glassmorphism |
| 5 | Users.razor | Permission checkbox tree, visual indicators |

### Target Desain Global
- [ ] **Skeleton Loading** — ganti semua loading spinner dengan skeleton placeholder
- [ ] **Empty States** — setiap halaman kosong punya ilustrasi SVG + CTA button
- [ ] **Dark Mode Toggle** — button toggle di topbar, persist ke localStorage
- [ ] **Sidebar** — collapse/expand, active state yang jelas, tooltip saat collapsed
- [ ] **Animations** — smooth transition pada page navigation dan dialog open/close

---

## ✅ Definition of Done (Per Phase)

- [ ] Unit test untuk Application layer (command/query handler baru)
- [ ] Integration test di `CleanArchitecture.Application.IntegrationTests`
- [ ] XML doc `<summary>` di semua public interface dan method baru
- [ ] `appsettings.json` diupdate dengan config section baru + inline comment
- [ ] `README.md` diupdate dengan section fitur baru
- [ ] `CHANGELOG.md` diupdate
- [ ] Template output tidak menyertakan file `docs/planning/` ini

---

## 📝 Development Notes & Keputusan Arsitektur

*(Isi bagian ini saat development berlangsung — catat keputusan penting, trade-off, dan gotchas)*

### Phase 1
- **Clean Architecture Boundary**: `CleanArchitecture.Application` tidak boleh mereferensi ASP.NET Core `HttpContext`. Abstraksi `IIdempotencyKeyAccessor` menjaga pipeline MediatR terisolasi dan portable.
- **DI Registration**: Gunakan `TryAddTransient` untuk registration fallback `NullIdempotencyKeyAccessor` agar Presentation layer bisa meng-override tanpa konflik urutan.
- **EF Core Multi-Provider**: Swapping dari SQL Server ke SQLite di `CustomWebApplicationFactory` memerlukan pembersihan descriptor EF Core internal agar provider option builder tidak bertubrukan.

### Phase 2
- **Atomic Change Auditing**: `AuditableEntityInterceptor` dijalankan sebelum `base.SavingChangesAsync`, sehingga entri `AuditLog` otomatis tersimpan dalam batch dan database transaction yang sama dengan entity yang dimutasi.
- **No Infinite Loop**: Interceptor secara eksplisit mengabaikan entity tipe `AuditLog` dan owned types, sehingga pencatatan audit log tidak memicu re-auditing berulang.
- **Cross-Database Value Conversion**: `AffectedColumns` disimpan dengan ValueConverter JSON di `AuditLogConfiguration`, menjamin kompatibilitas penuh antara SQL Server di production dan SQLite di automated test suite.
- **Integration Test Coverage**: `CustomWebApplicationFactory` mendaftarkan `AuditableEntityInterceptor` ke SQLite options sehingga mutation testing (Insert, Update, Soft-delete) otomatis terverifikasi end-to-end.

### Phase 3
- ...

### Phase 4
- ...

### Phase 5
- ...

---

*Plan ini dibuat pada: 2026-10-19*  
*Versi plan: 1.0*
