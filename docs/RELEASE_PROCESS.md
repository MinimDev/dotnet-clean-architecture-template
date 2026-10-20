# Panduan Rilis & Proteksi Publishing ke NuGet Gallery

Dokumen ini menjelaskan alur rilis resmi template `Minimdev.CleanArchitecture.Template`, konvensi penomoran versi (SemVer), serta cara mengamankan repository agar **hanya Lead Developer / Maintainer yang memiliki izin untuk membuat tag dan mem-publish package ke NuGet**.

---

## 1. Alur Rilis (Step-by-Step Release Flow)

Workflow publishing menggunakan **GitHub Actions** dengan autentikasi **NuGet OIDC Trusted Publishing (Keyless)** yang terdefinisi di [`.github/workflows/publish.yml`](../.github/workflows/publish.yml).

### Langkah 1: Persiapan Versi & Catatan Rilis
Sebelum melakukan rilis, pastikan file-file berikut telah diperbarui:
1. **`Minimdev.CleanArchitecture.Template.csproj`**:
   - Perbarui tag `<Version>` (misal `1.7.0-rc1` atau `1.7.0`).
   - Perbarui `<PackageReleaseNotes>` dengan ringkasan fitur baru.
2. **`CHANGELOG.md`**:
   - Tambahkan header versi baru sesuai format [Keep a Changelog](https://keepachangelog.com/).
3. **Dokumentasi Terkait**:
   - Pastikan `README.md` dan `docs/planning/` mencerminkan fitur terkini.

### Langkah 2: Verifikasi & Pengujian Lokal
Jalankan kompilasi, test suite, dan verifikasi pembuatan package:
```bash
# Pastikan semua unit test & integration test lolos
dotnet test CleanArchitecture.sln

# Verifikasi pembuatan package NuGet (.nupkg)
dotnet pack Minimdev.CleanArchitecture.Template.csproj -c Release
```

### Langkah 3: Commit & Push ke Branch `main`
```bash
git add .
git commit -m "chore(release): prepare v1.7.0-rc1"
git push origin main
```

### Langkah 4: Buat & Push Git Tag
Mendorong tag berformat `v*` akan secara otomatis memicu GitHub Actions untuk mem-pack dan mem-publish package ke NuGet:
```bash
# Buat git tag lokal
git tag v1.7.0-rc1

# Push tag ke remote repository
git push origin v1.7.0-rc1
```

### Langkah 5: Monitoring & Verifikasi Rilis
1. Buka tab **Actions** di repository GitHub Anda: `https://github.com/MinimDev/dotnet-clean-architecture-template/actions`.
2. Workflow **Publish to NuGet** akan berjalan otomatis:
   - Build & Pack template menjadi `.nupkg`.
   - OIDC Token Exchange via action `NuGet/login@v1`.
   - Push package ke NuGet Gallery (`api.nuget.org`).
   - Membuat GitHub Release resmi dengan release notes otomatis dan melampirkan `.nupkg`.
3. Cek publikasi di [NuGet Gallery - Minimdev.CleanArchitecture.Template](https://www.nuget.org/packages/Minimdev.CleanArchitecture.Template).

---

## 2. Cara Mengunci & Memproteksi Izin Pembuatan Tag (Tag Protection)

Secara default di GitHub, kolaborator dengan hak akses **Write** dapat membuat dan me-push git tag. Untuk membatasi agar **hanya Anda (Lead Developer/Owner)** yang dapat membuat tag `v*` dan memicu publikasi, ikuti panduan berikut:

### Opsi A: Menggunakan GitHub Repository Rulesets (Rekomendasi Modern)

GitHub Rulesets memberikan kontrol granular berdasarkan peran dan username tertentu:

1. Buka repository GitHub di browser: **Settings** $\to$ menu kiri **Rules** $\to$ pilih **Rulesets**.
2. Klik tombol **New ruleset** $\to$ pilih **New tag ruleset**.
3. Isi konfigurasi sebagai berikut:
   - **Ruleset Name**: `Protected Version Tags`
   - **Enforcement status**: Pilih **Active**.
4. **Target tags**:
   - Klik **Add target** $\to$ pilih **Include by pattern**.
   - Masukkan pola: `refs/tags/v*` (atau `v*`).
5. **Bypass list** *(PENTING - Menentukan siapa yang boleh membuat tag)*:
   - Klik **Add bypass**.
   - Pilih peran: **Repository admin** (hanya Admin yang boleh bypass), ATAU
   - Pilih akun spesifik Anda (misal username GitHub Anda).
6. **Rules (Aturan yang diterapkan)**:
   - Centang **Restrict creations** *(Mencegah user non-bypass membuat tag baru)*.
   - Centang **Restrict updates** *(Mencegah pemindahan/overwrite tag)*.
   - Centang **Restrict deletions** *(Mencegah penghapusan tag yang sudah dirilis)*.
7. Klik **Save changes**.

> [!IMPORTANT]
> Setelah aturan ini aktif, jika anggota tim lain mencoba menjalankan:
> ```bash
> git push origin v1.7.0
> ```
> GitHub akan langsung menolak dengan error:
> `remote: error: GH006: Protected tag 'refs/tags/v1.7.0' cannot be created by @developer.`

---

### Opsi B: Menggunakan Tag Protection Rules (GitHub Classic)

Jika repository Anda menggunakan antarmuka klasik GitHub:
1. Buka **Settings** $\to$ menu kiri **Tags**.
2. Klik **Add tag protection rule**.
3. Di kolom **Tag name pattern**, masukkan: `v*`.
4. Di bagian izin, secara default hanya pengguna dengan hak akses **Maintain** atau **Admin** yang diizinkan me-push tag. Anggota dengan hak akses **Write** (Developer biasa) akan diblokir.
5. Klik **Add rule**.

---

### Opsi C: Tambahan Approval Gate di GitHub Environment (Double Protection)

Jika Anda ingin ada tombol persetujuan manual (**Review & Approve**) sebelum package benar-benar dikirim ke NuGet Gallery:

1. Buka repository GitHub: **Settings** $\to$ **Environments** $\to$ **New environment**.
2. Beri nama: `nuget-release`.
3. Di bagian **Deployment protection rules**:
   - Centang **Required reviewers**.
   - Masukkan username GitHub Anda.
4. Di file [`.github/workflows/publish.yml`](../.github/workflows/publish.yml), tambahkan baris `environment: nuget-release` pada job publish:
   ```yaml
   jobs:
     publish:
       name: Pack & Publish
       runs-on: ubuntu-latest
       environment: nuget-release  # <--- Membutuhkan persetujuan manual Anda
       permissions:
         id-token: write
         contents: write
   ```
5. Hasilnya: Setiap kali tag di-push, pipeline akan berhenti sejenak dan mengirim notifikasi email ke Anda untuk menekan tombol **"Review deployments" $\to$ "Approve and deploy"**.

---

## 3. Konvensi Penamaan Tag (SemVer 2.0)

Gunakan format Semantic Versioning diawali huruf `v`:

| Jenis Rilis | Format Tag | Contoh | Kegunaan |
| :--- | :--- | :--- | :--- |
| **Alpha** | `vX.Y.Z-alpha` | `v1.7.0-alpha` | Fitur tahap awal (misal: eksperimen pipeline) |
| **Beta** | `vX.Y.Z-beta` | `v1.7.0-beta` | Fitur fungsional untuk pengujian integrasi |
| **Release Candidate** | `vX.Y.Z-rcN` | `v1.7.0-rc1` | Pra-rilis stabil sebelum rilis final |
| **General Availability** | `vX.Y.Z` | `v1.7.0` | Versi resmi untuk production |
| **Patch / Hotfix** | `vX.Y.Z` | `v1.7.1` | Perbaikan bug tanpa breaking change |

---

## 4. Troubleshooting

- **Error `NU5037: The package is missing required metadata`**:
  Pastikan `Minimdev.CleanArchitecture.Template.csproj` memiliki elemen `PackageId`, `Authors`, `Description`, dan `PackageLicenseExpression`.
- **Error `403 Forbidden` saat NuGet Push**:
  Pastikan akun Trusted Publisher di [NuGet Account Trusted Publishing](https://www.nuget.org/account/trustedpublishing) sesuai dengan nama repository GitHub (`MinimDev/dotnet-clean-architecture-template`).
- **Tag lokal salah**:
  Jika salah memberi nama tag sebelum di-push:
  ```bash
  git tag -d v1.7.0-salah
  ```
