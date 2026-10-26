# Release Guide & NuGet Gallery Publishing Protection

This document explains the official release flow for `Minimdev.CleanArchitecture.Template`, version numbering conventions (SemVer), and how to secure the repository so that **only the Lead Developer / Maintainer has permission to create release tags and publish packages to NuGet**.

---

## 1. Step-by-Step Release Flow

The publishing workflow uses **GitHub Actions** with **NuGet OIDC Trusted Publishing (Keyless)** authentication defined in [`.github/workflows/publish.yml`](../.github/workflows/publish.yml).

### Step 1: Version Preparation & Release Notes
Before initiating a release, ensure the following files are updated:
1. **`Minimdev.CleanArchitecture.Template.csproj`**:
   - Update `<Version>` (e.g., `1.8.0-rc1` or `1.8.0`).
   - Update `<PackageReleaseNotes>` with a summary of new features and fixes.
2. **`CHANGELOG.md`**:
   - Add the new version section following [Keep a Changelog](https://keepachangelog.com/).
3. **Associated Documentation**:
   - Ensure `README.md` and `docs/` reflect the latest capabilities.

### Step 2: Local Verification & Testing
Execute compilation, the test suite, and verify package creation:
```bash
# Ensure all unit tests & integration tests pass
dotnet test CleanArchitecture.sln

# Verify NuGet package creation (.nupkg)
dotnet pack Minimdev.CleanArchitecture.Template.csproj -c Release
```

### Step 3: Commit & Push to `main` Branch
```bash
git add .
git commit -m "chore(release): prepare v1.8.0"
git push origin main
```

### Step 4: Create & Push Git Tag
Pushing a tag matching the `v*` pattern automatically triggers the GitHub Actions workflow to pack and publish the package to NuGet:
```bash
# Create local git tag
git tag v1.8.0

# Push tag to remote repository
git push origin v1.8.0
```

### Step 5: Monitoring & Release Verification
1. Open the **Actions** tab in your GitHub repository: `https://github.com/MinimDev/dotnet-clean-architecture-template/actions`.
2. The **Publish to NuGet** workflow will run automatically:
   - Build & Pack template into `.nupkg`.
   - Perform OIDC Token Exchange via the `NuGet/login@v1` action.
   - Push package to NuGet Gallery (`api.nuget.org`).
   - Create an official GitHub Release with automated release notes and attach the `.nupkg` artifact.
3. Verify the published package on [NuGet Gallery - Minimdev.CleanArchitecture.Template](https://www.nuget.org/packages/Minimdev.CleanArchitecture.Template).

---

## 2. Locking & Protecting Tag Creation Permissions (Tag Protection)

By default on GitHub, collaborators with **Write** access can create and push git tags. To restrict tag creation so that **only you (Lead Developer / Owner)** can create `v*` tags and trigger releases, follow the steps below:

### Option A: Using GitHub Repository Rulesets (Modern Recommended)

GitHub Rulesets provide granular, role- and user-based control:

1. Open your GitHub repository in your browser: **Settings** $\to$ left sidebar **Rules** $\to$ select **Rulesets**.
2. Click **New ruleset** $\to$ select **New tag ruleset**.
3. Configure the ruleset:
   - **Ruleset Name**: `Protected Version Tags`
   - **Enforcement status**: Set to **Active**.
4. **Target tags**:
   - Click **Add target** $\to$ select **Include by pattern**.
   - Enter pattern: `refs/tags/v*` (or `v*`).
5. **Bypass list** *(IMPORTANT - Defines who is allowed to create tags)*:
   - Click **Add bypass**.
   - Select role: **Repository admin** (only Admins can bypass), OR
   - Select your specific GitHub account username.
6. **Rules (Enforced restrictions)**:
   - Check **Restrict creations** *(Prevents non-bypass users from creating new tags)*.
   - Check **Restrict updates** *(Prevents moving or overwriting existing tags)*.
   - Check **Restrict deletions** *(Prevents deletion of published release tags)*.
7. Click **Save changes**.

> [!IMPORTANT]
> Once this rule is active, if another team member attempts to run:
> ```bash
> git push origin v1.8.0
> ```
> GitHub will immediately reject the push with an error:
> `remote: error: GH006: Protected tag 'refs/tags/v1.8.0' cannot be created by @developer.`

---

### Option B: Using Tag Protection Rules (GitHub Classic)

If your repository uses the classic GitHub interface:
1. Navigate to **Settings** $\to$ left sidebar **Tags**.
2. Click **Add tag protection rule**.
3. In the **Tag name pattern** field, enter: `v*`.
4. In permissions, by default only users with **Maintain** or **Admin** permissions are allowed to push tags. Users with **Write** access (standard developers) will be blocked.
5. Click **Add rule**.

---

### Option C: Additional Approval Gate via GitHub Environment (Double Protection)

If you require a manual approval gate (**Review & Approve**) before the package is dispatched to NuGet Gallery:

1. In your GitHub repository: **Settings** $\to$ **Environments** $\to$ **New environment**.
2. Name the environment: `nuget-release`.
3. Under **Deployment protection rules**:
   - Check **Required reviewers**.
   - Enter your GitHub username.
4. In [`.github/workflows/publish.yml`](../.github/workflows/publish.yml), add the `environment: nuget-release` property to the publish job:
   ```yaml
   jobs:
     publish:
       name: Pack & Publish
       runs-on: ubuntu-latest
       environment: nuget-release  # <--- Requires your manual approval
       permissions:
         id-token: write
         contents: write
   ```
5. Outcome: Whenever a tag is pushed, the pipeline pauses and dispatches an email notification requesting you to click **"Review deployments" $\to$ "Approve and deploy"**.

---

## 3. Tag Naming Conventions (SemVer 2.0)

Use Semantic Versioning prefixed with the letter `v`:

| Release Type | Tag Format | Example | Purpose |
| :--- | :--- | :--- | :--- |
| **Alpha** | `vX.Y.Z-alpha` | `v1.8.0-alpha` | Early-stage features (e.g., pipeline or experimental changes) |
| **Beta** | `vX.Y.Z-beta` | `v1.8.0-beta` | Feature-complete builds for integration testing |
| **Release Candidate** | `vX.Y.Z-rcN` | `v1.8.0-rc1` | Feature-frozen stable candidate prior to final release |
| **General Availability** | `vX.Y.Z` | `v1.8.0` | Official stable production release |
| **Patch / Hotfix** | `vX.Y.Z` | `v1.8.1` | Bug fixes without breaking changes |

---

## 4. Troubleshooting

- **Error `NU5037: The package is missing required metadata`**:
  Ensure `Minimdev.CleanArchitecture.Template.csproj` contains `PackageId`, `Authors`, `Description`, and `PackageLicenseExpression` elements.
- **Error `403 Forbidden` on NuGet Push**:
  Verify that the Trusted Publisher account in [NuGet Account Trusted Publishing](https://www.nuget.org/account/trustedpublishing) matches your GitHub repository name (`MinimDev/dotnet-clean-architecture-template`).
- **Incorrect local tag**:
  If a tag was created locally with an error before pushing:
  ```bash
  git tag -d v1.8.0-wrong
  ```
