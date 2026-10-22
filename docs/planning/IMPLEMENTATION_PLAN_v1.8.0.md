# 🚀 Implementation Plan — Clean Architecture Template v1.8.0 (UI/UX Polish & Real-Time Notifications)

> **Versi Target:** 1.8.0  
> **Estimasi Total:** 5 sesi kerja  
> **Tanggal Dibuat:** 2026-10-22  
> **Filosofi:** Desain UI/UX kelas enterprise yang cepat, responsif, dan konsisten — state terpusat di WebUI services, reusable shared components, dan feedback loops yang instan sebagai starter template production-ready.

---

## 📊 Status Keseluruhan

| Phase | Fitur / Area | Status | Branch |
|---|---|---|---|
| 1 | Real-Time Notification System & Immediate Badge Update | 🔄 Sedang Dikerjakan | `feat/notification-realtime` |
| 2 | Modern Application Shell (Command Palette Ctrl+K & Breadcrumbs) | ⬜ Belum Mulai | `feat/ui-app-shell` |
| 3 | Shared Reusable Component Library (PageHeader, EmptyState, ConfirmDialog) | ⬜ Belum Mulai | `feat/ui-shared-components` |
| 4 | Enterprise CRUD Pages Polish (Products, Users/PBAC, AuditLogs, Settings) | ⬜ Belum Mulai | `feat/ui-crud-polish` |
| 5 | Design Tokens, Micro-Interactions & Mobile Optimization | ⬜ Belum Mulai | `feat/ui-design-tokens` |

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
**Status:** 🔄 Sedang Dikerjakan

### 🎯 Tujuan
Memperbaiki issue badge notifikasi yang tidak langsung berubah saat ada notifikasi baru, menghilangkan delay polling 30 detik, serta menyediakan state management notifikasi yang reaktif di WebUI.

### 🐛 Root Cause yang Sudah Dianalisis
1. **Long Polling Interval:** Polling di `MainLayout.razor` menggunakan interval 30 detik (`PeriodicTimer(TimeSpan.FromSeconds(30))`).
2. **Tidak Ada Trigger saat Navigasi:** Pergantian halaman (`NavigationManager.LocationChanged`) tidak memicu re-fetch unread count.
3. **Ketiadaan Centralized Notification State:** Halaman CRUD (seperti `Products.razor` atau `Users.razor`) yang menghasilkan event/notifikasi tidak memiliki mekanisme untuk memberitahu `MainLayout` secara instan.
4. **Desinkronisasi State di `NotificationPanel`:** Komponen `NotificationPanel.razor` tidak mengimplementasikan `OnParametersSet()`, sehingga jika `UnreadCount` di `MainLayout` berubah, panel tidak ter-*render* ulang.

### ✅ Checklist Pekerjaan

#### A. Centralized State Service (WebUI)
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Services/NotificationStateService.cs`
  - Menyimpan state `UnreadCount` dan `Notifications` list aktif.
  - Event `event Action? OnChange;` untuk reaktivitas Blazor.
  - Method `Task RefreshAsync()` untuk memanggil API `unread-count` dan menembak `OnChange`.
  - Method `void DecrementUnread()` dan `void ResetUnread()` untuk *optimistic UI update*.
  - Method `Task MarkAsReadAsync(Guid id)` dan `Task MarkAllAsReadAsync()`.
- [ ] Daftarkan `NotificationStateService` sebagai Scoped Service di `Program.cs`.

#### B. MainLayout Integration
- [ ] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Layout/MainLayout.razor`
  - Inject `NotificationStateService` menggantikan pemanggilan polling manual.
  - Hook ke `NavigationManager.LocationChanged` agar unread count otomatis di-refresh setiap kali user berpindah halaman.
  - Turunkan background timer fallback ke **15 detik**.
  - Tambahkan animasi CSS *badge pulse/bounce* saat unread count bertambah (`@key="_unreadCount"`).

#### C. NotificationPanel Refactoring
- [ ] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/NotificationPanel.razor`
  - Konsumsi `NotificationStateService` secara langsung.
  - Implementasikan `OnParametersSet()` untuk sinkronisasi state dua arah.
  - Tambahkan indikator loading skeleton saat pertama kali dibuka.
  - Tambahkan tombol quick action "Mark all as read" di header popover.

---

## Phase 2 — Modern Application Shell & Global Navigation
**Target Versi:** `v1.8.0-beta`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-app-shell`  
**Status:** ⬜ Belum Mulai

### 🎯 Tujuan
Membangun layout shell modern kelas enterprise dengan navigasi kilat (Command Palette `Ctrl+K`), breadcrumbs otomatis, dan sidebar yang fleksibel.

### ✅ Checklist Pekerjaan

#### A. Command Palette / Global Search (`Ctrl + K`)
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/CommandPalette.razor`
  - Shortcut keyboard global listener (`Ctrl + K` / `Cmd + K`).
  - Search dialog mengambang (Spotlight search style).
  - Indeks pencarian otomatis: Halaman Menu, Fitur, Dokumen Hangfire/Scalar, dan Aksi Cepat (misal: "Create Product", "Manage Roles").
  - Navigasi instan via panah keyboard (`Up/Down/Enter`).

#### B. Dynamic Breadcrumbs Bar
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/AppBreadcrumbs.razor`
  - Parsing rute aktif secara dinamis (`/products` → `Dashboard / Products`, `/products/create` → `Dashboard / Products / New`).
  - Integrasikan ke dalam `MainLayout.razor` tepat di bawah Appbar atau di atas `@Body`.

#### C. Refined Collapsible Sidebar & Header
- [ ] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Layout/NavMenu.razor`
  - Tambahkan tooltip pada item menu saat sidebar dalam mode collapsed (mini-sidebar).
  - Tambahkan badge notifikasi jumlah item pada link menu (misal: badge count pada menu jika ada tugas tertunda).
- [ ] **UBAH** `src/Presentation/CleanArchitecture.WebUI/Components/Layout/MainLayout.razor`
  - Tambahkan shortcut button pencarian `Ctrl + K` di Topbar (tampilan tombol pencarian modern seperti GitHub/Linear).

---

## Phase 3 — Shared Reusable Component Library
**Target Versi:** `v1.8.0-rc1`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-shared-components`  
**Status:** ⬜ Belum Mulai

### 🎯 Tujuan
Menghilangkan duplikasi markup UI di seluruh halaman dengan membangun komponen reusable berstandar tinggi.

### ✅ Checklist Pekerjaan

#### A. `PageHeader.razor`
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/PageHeader.razor`
  - Parameter: `Title`, `Subtitle`, `Icon`, `RenderFragment Actions`, `RenderFragment Breadcrumbs`.
  - Tampilan visual konsisten dengan subtle gradient accent, flex responsif untuk tombol aksi.

#### B. `EmptyState.razor`
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/EmptyState.razor`
  - Parameter: `Icon`, `Title`, `Description`, `ActionText`, `EventCallback OnActionClick`.
  - Ilustrasi SVG tematik yang ramah, bukan sekadar teks abu-abu polos.

#### C. `ConfirmDeleteDialog.razor`
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/ConfirmDeleteDialog.razor`
  - Dialog modal standar untuk aksi destruktif (Delete / Deactivate).
  - Tombol aksi dengan proteksi double-click + loading indicator + visual warning tegas.

#### D. `TableSkeleton.razor` & `StatCard.razor`
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/TableSkeleton.razor` untuk placeholder loading tabel.
- [ ] **BUAT** `src/Presentation/CleanArchitecture.WebUI/Components/Shared/StatCard.razor` untuk kartu ringkasan metrik / KPI di dashboard.

---

## Phase 4 — Enterprise CRUD Pages Polish
**Target Versi:** `v1.8.0-rc2`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-crud-polish`  
**Status:** ⬜ Belum Mulai

### 🎯 Tujuan
Menerapkan standar komponen baru ke seluruh modul inti template (`Products`, `Users`, `Audit Trail`, `App Settings`).

### ✅ Checklist Pekerjaan

#### A. Products Module (`Products.razor`)
- [ ] Implementasikan View Mode Switcher: **Grid Card View** vs **Data Table View**.
- [ ] Input search dengan *debounce* 350ms agar efisien terhadap API call.
- [ ] Filter Chips: Status (`Active`, `Draft`, `Archived`), Rentang Harga.
- [ ] Pasang `PageHeader.razor` dan `EmptyState.razor`.

#### B. Users & PBAC Module (`Users.razor`)
- [ ] Terapkan visual badges modern untuk Role & Permissions.
- [ ] Tambahkan search filter instan pada PBAC permission matrix.
- [ ] Tambahkan dirty state guard: konfirmasi jika user berpindah tab tanpa menyimpan matrix permission.

#### C. Audit Trail Module (`AuditLogs.razor`)
- [ ] Modal JSON Diff Viewer interaktif: visual pembanding data `Before` vs `After` yang mudah dibaca non-developer.
- [ ] Filter tanggal cepat (*Quick Range: Today, Last 7 Days, This Month*).

#### D. App Settings Module (`AppSettings.razor`)
- [ ] Pengelompokan setting berdasarkan Kategori/Tab (*General, Security, Features, Notifications*).
- [ ] Indikator dirty state per field (tanda perubahan yang belum disimpan).

---

## Phase 5 — Design System, Micro-Interactions & Mobile Optimization
**Target Versi:** `v1.8.0`  
**Estimasi:** 1 sesi  
**Branch:** `feat/ui-design-tokens`  
**Status:** ⬜ Belum Mulai

### 🎯 Tujuan
Finishing touch pada estetika, kontras warna Dark/Light mode, transisi animasi halus, dan responsivitas mobile.

### ✅ Checklist Pekerjaan

#### A. Design Tokens & Styling
- [ ] Sempurnakan `wwwroot/css/app.css`:
  - Variabel HSL untuk warna Surface, Border, dan Primary glow.
  - Dark mode contrast refinement: pastikan teks abu-abu tetap memiliki rasio kontras WCAG AA (minimal 4.5:1).
  - Micro-animations untuk card hover, dialog zoom-in, dan table row transition.

#### B. Mobile & Tablet Responsiveness
- [ ] Uji dan optimasi drawer navigasi di layar mobile (< 600px).
- [ ] Optimasi ukuran tombol touch target (minimal 44x44px).
- [ ] Ubah dialog modal menjadi full-width bottom-sheet di layar smartphone.

---

## 🧪 Verifikasi & Kriteria Keberhasilan (Definition of Done)

1. **Notifikasi:**
   - [ ] Saat notifikasi baru dibuat di backend, badge unread count di Topbar langsung bertambah dalam waktu < 1 detik setelah navigasi atau < 15 detik saat idle.
   - [ ] Badge langsung berkurang atau hilang instan saat notifikasi di-*mark as read*.
2. **Pencarian Cepat:**
   - [ ] Menekan `Ctrl + K` di halaman mana pun langsung membuka Command Palette dan navigasi bekerja mulus.
3. **Standarisasi:**
   - [ ] Seluruh halaman CRUD menggunakan `PageHeader` dan `EmptyState` yang seragam.
   - [ ] Tidak ada duplikasi style CSS ad-hoc di file `.razor`.
4. **Build & Tests:**
   - [ ] Solusi build sukses tanpa warning/error (`dotnet build` 0 Error).
   - [ ] Seluruh integration & unit test tetap lolos 100%.
