# 🚀 Implementation Plan — Clean Architecture Template v1.8.0 (UI/UX Polish & Real-Time Notifications)

> **Versi Target:** 1.8.0  
> **Estimasi Total:** 5 sesi kerja  
> **Tanggal Dibuat:** 2026-10-22  
> **Filosofi:** Desain UI/UX kelas enterprise yang cepat, responsif, dan konsisten — state terpusat di WebUI services, reusable shared components, dan feedback loops yang instan sebagai starter template production-ready.

---

## 📊 Status Keseluruhan

| Phase | Fitur / Area | Status | Branch |
|---|---|---|---|
| 1 | Real-Time Notification System & Immediate Badge Update | ✅ Selesai (`4dd6ab9`) | `feat/notification-realtime` |
| 2 | Modern Application Shell (Command Palette Ctrl+K, Breadcrumbs, Theme Presets, Login Polish) | ✅ Selesai (`dfe16ba`) | `feat/ui-app-shell` |
| 3 | Shared Reusable Component Library (PageHeader, EmptyState, ConfirmDialog, TableSkeleton, StatCard) | ✅ Selesai (`8af5b07`) | `feat/ui-shared-components` |
| 4 | Enterprise CRUD Pages Polish (Products, Users/PBAC, AuditLogs, Settings) | ✅ Selesai (`c122106`) | `feat/ui-shared-components` |
| 5 | Design Tokens, Micro-Interactions & Mobile Optimization | ✅ Selesai (`457bed1`) | `feat/ui-shared-components` |

**Status Legend:** ⬜ Belum Mulai · 🔄 Sedang Dikerjakan · ✅ Selesai · 🚧 Blocked

---

## 🗺️ Overview Milestone

```
v1.7.0 (saat ini)
    │
    ├── Phase 1: Notification & State Management
    │   └── [v1.8.0-alpha] Immediate Badge Update & NotificationStateService
    │
    ├── Phase 2: Navigation & Shell Overhaul
    │   └── [v1.8.0-beta] Command Palette (Ctrl+K), Breadcrumbs & Mini-Sidebar
    │
    ├── Phase 3: Component Standardization
    │   └── [v1.8.0-rc1] PageHeader, EmptyState, ConfirmDialog & TableSkeleton
    │
    ├── Phase 4: Domain Pages Polish
    │   └── [v1.8.0-rc2] Products View Toggle, PBAC UX, Audit Diff & Settings Tabs
    │
    └── Phase 5: Design System Polish
        └── [v1.8.0] HSL Color Balance, Micro-animations & Mobile Responsiveness
```

---

## Phase 1 — Real-Time Notification System & Immediate Badge Update
**Target Versi:** `v1.8.0-alpha`  
**Estimasi:** 1 sesi  
**Branch:** `feat/notification-realtime`  
**Status:** ✅ Selesai

### 🎯 Tujuan
Memperbaiki issue badge notifikasi yang tidak langsung berubah saat ada notifikasi baru, menghilangkan delay polling 30 detik, serta menyediakan state management notifikasi yang reaktif di WebUI.

### 🐛 Root Cause yang Sudah Dianalisis
1. **Long Polling Interval:** Polling di `MainLayout.razor` menggunakan interval 30 detik (`PeriodicTimer(TimeSpan.FromSeconds(30))`).
2. **Tidak Ada Trigger saat Navigasi:** Pergantian halaman (`NavigationManager.LocationChanged`) tidak memicu re-fetch unread count.
3. **Ketiadaan Centralized Notification State:** Halaman CRUD (seperti `Products.razor` atau `Users.razor`) yang menghasilkan event/notifikasi tidak memiliki mekanisme untuk memberitahu `MainLayout` secara instan.
4. **Desinkronisasi State di `NotificationPanel`:** Komponen `NotificationPanel.razor` tidak mengimplementasikan `OnParametersSet()`, sehingga jika `UnreadCount` di `MainLayout` berubah, panel tidak ter-*render* ulang.

### ✅ Checklist Pekerjaan

#### A. Centralized State Service (WebUI)
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Services/NotificationStateService.cs`
  - Menyimpan state `UnreadCount` dan `Notifications` list aktif.
  - Event `event Action? OnChange;` untuk reaktivitas Blazor.
  - Method `Task RefreshAsync()` untuk memanggil API `unread-count` dan menembak `OnChange`.
  - Method `void DecrementUnread()` dan `void ResetUnread()` untuk *optimistic UI update*.
  - Method `Task MarkAsReadAsync(Guid id)` dan `Task MarkAllAsReadAsync()`.
- [x] Daftarkan `NotificationStateService` sebagai Scoped Service di `Program.cs`.

#### B. MainLayout Integration
- [x] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Layout/MainLayout.razor`
  - Inject `NotificationStateService` menggantikan pemanggilan polling manual.
  - Hook ke `NavigationManager.LocationChanged` agar unread count otomatis di-refresh setiap kali user berpindah halaman.
  - Turunkan background timer fallback ke **15 detik**.
  - Tambahkan animasi CSS *badge pulse/bounce* saat unread count bertambah (`@key="_unreadCount"`).

#### C. NotificationPanel Refactoring
- [x] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/NotificationPanel.razor`
  - Konsumsi `NotificationStateService` secara langsung.
  - Implementasikan `OnParametersSet()` untuk sinkronisasi state dua arah.
  - Tambahkan indikator loading skeleton saat pertama kali dibuka.
  - Tambahkan tombol quick action "Mark all as read" di header popover.

---

## Phase 2 — Modern Application Shell & Global Navigation
**Target Versi:** `v1.8.0-beta`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-app-shell`  
**Status:** ✅ Selesai (`commit dfe16ba`)

### 🎯 Tujuan
Membangun layout shell modern kelas enterprise dengan navigasi kilat (Command Palette `Ctrl+K`), breadcrumbs otomatis, theme preset manager, responsive layout presisi, serta penyempurnaan halaman login bebas efek AI glow.

### ✅ Checklist Pekerjaan

#### A. Command Palette / Global Search (`Ctrl + K`)
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/CommandPalette.razor`
  - Shortcut keyboard global listener (`Ctrl + K` / `Cmd + K`).
  - Search dialog mengambang (Spotlight search style).
  - Indeks pencarian otomatis: Halaman Menu, Fitur, Dokumen Hangfire/Scalar, dan Aksi Cepat.
  - Navigasi instan via panah keyboard (`Up/Down/Enter`).

#### B. Dynamic Breadcrumbs Bar
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/AppBreadcrumbs.razor`
  - Parsing rute aktif secara dinamis (`/products` → `Home / Products`, dst).
  - Integrasikan ke dalam `MainLayout.razor` di atas container konten.

#### C. Refined Collapsible Sidebar & Header
- [x] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Layout/NavMenu.razor`
  - Bersihkan profil card ganda di bagian bawah sidebar.
  - Perbaiki drawer clip mode agar konten halaman tidak tertutup.
- [x] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Layout/MainLayout.razor`
  - Pasang shortcut button pencarian `Ctrl + K` di Topbar.
  - Tambahkan `ThemePickerMenu` untuk switch antara 6 preset tema enterprise.
  - Perbaiki padding responsive horizontal (`px-4 px-sm-5 px-md-6`) agar simetris kiri-kanan pada semua ukuran layar.

#### D. Login Page & Dark Mode Enterprise Refinement
- [x] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Pages/Login.razor` & `AuthLayout.razor`
  - Hapus semua efek *glowing* ungu / *AI slop* (`radial-gradient` magenta/ungu mengambang).
  - Ganti dengan tema **Enterprise Cobalt Blue & Dark Slate** (`#0b0f19` & `#111827`).
  - Standardisasi seluruh teks dan pesan error menjadi **100% Bahasa Inggris**.
  - Sediakan tombol demo credentials one-click (Administrator & Standard Member).

---

## Phase 3 — Shared Reusable Component Library
**Target Versi:** `v1.8.0-rc1`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-shared-components`  
**Status:** ✅ Selesai (`8af5b07`)

### 🎯 Tujuan
Menghilangkan duplikasi markup UI di seluruh halaman dengan membangun komponen reusable berstandar tinggi.

### ✅ Checklist Pekerjaan

#### A. `PageHeader.razor`
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/PageHeader.razor`
  - Parameter: `Title`, `Subtitle`, `Icon`, `RenderFragment Actions`, `RenderFragment Breadcrumbs`.
  - Tampilan visual konsisten dengan subtle gradient accent, flex responsif untuk tombol aksi.

#### B. `EmptyState.razor`
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/EmptyState.razor`
  - Parameter: `Icon`, `Title`, `Description`, `ActionText`, `EventCallback OnActionClick`.
  - Ilustrasi SVG tematik yang ramah, bukan sekadar teks abu-abu polos.

#### C. `ConfirmDeleteDialog.razor`
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/ConfirmDeleteDialog.razor`
  - Dialog modal standar untuk aksi destruktif (Delete / Deactivate).
  - Tombol aksi dengan proteksi double-click + loading indicator + visual warning tegas.

#### D. `TableSkeleton.razor` & `StatCard.razor`
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/TableSkeleton.razor` untuk placeholder loading tabel.
- [x] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/StatCard.razor` untuk kartu ringkasan metrik / KPI di dashboard.

---

## Phase 4 — Enterprise CRUD Pages Polish
**Target Versi:** `v1.8.0-rc2`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-shared-components`  
**Status:** ✅ Selesai (`c122106`)

### 🎯 Tujuan
Menerapkan standar komponen baru ke seluruh modul inti template (`Products`, `Users`, `Audit Trail`, `App Settings`).

### ✅ Checklist Pekerjaan

#### A. Products Module (`Products.razor`)
- [x] Implementasikan View Mode Switcher: **Grid Card View** vs **Data Table View**.
- [x] Input search dengan *debounce* / live query.
- [x] Standardisasi dialog konfirmasi hapus via `ConfirmDeleteDialog`.
- [x] Pasang `PageHeader.razor` dan `EmptyState.razor`.

#### B. Users & PBAC Module (`Users.razor`)
- [x] Terapkan visual badges modern untuk Role & Permissions.
- [x] Tambahkan search filter instan pada PBAC permission matrix.
- [x] Tambahkan dirty state guard: indikator jika ada modifikasi matrix permission yang belum disimpan.
- [x] Gunakan `PageHeader.razor`, `TableSkeleton.razor`, dan `EmptyState.razor`.

#### C. Audit Trail Module (`AuditLogs.razor`)
- [x] Modal JSON Diff Viewer interaktif: visual pembanding data `Before` vs `After` yang mudah dibaca non-developer.
- [x] Filter chips untuk action type & entity search.
- [x] Pasang `PageHeader.razor`, `TableSkeleton.razor`, dan `EmptyState.razor`.

#### D. App Settings Module (`AppSettings.razor`)
- [x] Pengelompokan setting berdasarkan Group Category chips & search bar.
- [x] Indikator dirty state per field (tanda perubahan yang belum disimpan).
- [x] Pasang `PageHeader.razor`, `TableSkeleton.razor`, dan `EmptyState.razor`.

---

## Phase 5 — Design System, Micro-Interactions & Mobile Optimization
**Target Versi:** `v1.8.0`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-shared-components`  
**Status:** ✅ Selesai (`457bed1`)

### 🎯 Tujuan
Finishing touch pada estetika, kontras warna Dark/Light mode, transisi animasi halus, dan responsivitas mobile.

### ✅ Checklist Pekerjaan

#### A. Design Tokens & Styling
- [x] Sempurnakan `wwwroot/css/app.css`:
  - Micro-animations untuk card hover, dialog zoom-in entrance (`dialogEntrance`), dan table row transition.
  - Dark mode contrast refinement pada table row dan popovers.

#### B. Mobile & Tablet Responsiveness
- [x] Uji dan optimasi drawer navigasi di layar mobile (< 600px).
- [x] Optimasi ukuran tombol touch target (minimal 32-44px).
- [x] Penyesuaian margin & max-width dialog modal di layar smartphone (`max-width: calc(100vw - 24px)`).

---

## 🧪 Verifikasi & Kriteria Keberhasilan (Definition of Done)

1. **Notifikasi:**
   - [x] Saat notifikasi baru dibuat di backend, badge unread count di Topbar langsung bertambah dalam waktu < 1 detik setelah navigasi atau < 15 detik saat idle.
   - [x] Badge langsung berkurang atau hilang instan saat notifikasi di-*mark as read*.
2. **Pencarian Cepat:**
   - [x] Menekan `Ctrl + K` di halaman mana pun langsung membuka Command Palette dan navigasi bekerja mulus.
3. **Standarisasi:**
   - [x] Seluruh halaman CRUD menggunakan `PageHeader`, `EmptyState`, dan `TableSkeleton` yang seragam.
   - [x] Tidak ada duplikasi style CSS ad-hoc di file `.razor`.
4. **Build & Tests:**
   - [x] Solusi build sukses tanpa error (`dotnet build` 0 Error).
   - [x] Seluruh integration & unit test tetap lolos 100% (39/39 tests passed).
