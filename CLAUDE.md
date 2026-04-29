# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands
- **Restore dependencies**: `dotnet restore`
- **Build solution**: `dotnet build`
- **Run API (Backend)**: `dotnet run --project Menu.Api`
- **Run UI (Frontend)**: `dotnet run --project Menu.UI`
- **Apply Database Migrations**: `dotnet ef database update --project Menu.Infrastructure --startup-project Menu.Api`
- **Add New Migration**: `dotnet ef migrations add <<MigrationMigrationName> --project Menu.Infrastructure --startup-project Menu.Api`

## Architecture Overview
The project follows a clean, layered architecture pattern:

- **Menu.Api**: The entry point. Contains REST controllers, JWT authentication/authorization middleware, and global exception handling.
- **Menu.Application**: Orchestration layer. Contains business logic, DTOs, FluentValidation rules, and AutoMapper profiles.
- **Menu.Domain**: The core. Contains entities, enums, and repository interfaces. No dependencies on other layers.
- **Menu.Infrastructure**: Implementation layer. Contains the EF Core `DbContext`, SQL Server repository implementations, token services, and database seeding.
- **Menu.UI**: Blazor WebAssembly frontend utilizing MudBlazor for the UI components and typed service clients for API communication.

## Key Technical Details
- **Authentication**: JWT Bearer tokens. Authorization is managed via custom permission attributes on API endpoints.
- **Database**: SQL Server via Entity Framework Core. Migrations are managed in the `Menu.Infrastructure` project.
- **UI Framework**: MudBlazor.
- **Localization**: Supports multiple languages (including Arabic/RTL) managed via `AppState` and `LanguageToggle` components.
- **Public API**: A dedicated set of endpoints (`/api/public/*`) exists for non-authenticated public menu browsing.

---

## Latest Technical Work Summary (Debugging/Repair Session)

### 1. Current Project Type and Architecture
- **Backend:** ASP.NET Core Web API (Clean Architecture: Domain, Application, Infrastructure, Api)
- **Frontend:** Blazor WebAssembly (WASM) using MudBlazor for UI components.

### 2. Original Critical Startup Problem
The Blazor WASM application was freezing on startup during `await builder.Build().RunAsync();`. The root causes were a circular dependency in Dependency Injection and a cooperative thread deadlock during the initial auth state resolution.

### 3. All Files Reviewed So Far
- **Startup / DI:** `Program.cs`, `App.razor`
- **Auth Services:** `CustomAuthStateProvider.cs`, `JwtAuthorizationHandler.cs`, `UserContextService.cs`
- **UI Components:** `AuthorizedButton.razor`, `PermissionView.razor`, `AuthorizationExamples.razor`, `NavMenu.razor`, `MainLayout.razor`
- **Domain/API:** `PermissionNames.cs`, `RoleNames.cs`, `JwtClaimTypes.cs`, various API Controllers.
- **Pages:** `Menus/Index.razor`, `Users/Index.razor`, `Restaurants/Index.razor`, `Roles/Index.razor`.

### 4. Files Marked SAFE
- `App.razor` (AppState initialization correctly placed in `OnAfterRenderAsync`).
- Domain layer models/constants (used as source of truth).
- API controllers (properly applying role and permission policies).

### 5. Files Modified / Fixed
- `State/UserContextService.cs`
- `Auth/CustomAuthStateProvider.cs`
- `Auth/JwtAuthorizationHandler.cs`
- `Components/AuthorizedButton.razor`
- `Components/PermissionView.razor`
- `Pages/AuthorizationExamples.razor`
- `Auth/AppPermissions.cs` (New)
- `Auth/AppRoles.cs` (New)
- `Services/AuthorizationService.cs` (New)
- `Program.cs`
- `Layout/NavMenu.razor`
- `Layout/MainLayout.razor`
- All CRUD Pages (`Menus`, `Users`, `Restaurants`, `Roles`)

### 6. Exact Technical Issues Discovered
- **Circular DI:** `UserContextService` had a constructor dependency on `AuthenticationStateProvider`. `CustomAuthStateProvider` depended on `UserContextService`. This created a silent stack overflow / endless loop on startup.
- **Semaphore Leak:** `JwtAuthorizationHandler.cs` used `SemaphoreSlim.WaitAsync` without a timeout or `OperationCanceledException` handling, causing post-login deadlocks.
- **Async Overhead in Rendering:** `UserContextService` exposed `async` methods triggering re-evaluation of auth state during UI rendering, risking deadlocks.
- **Frontend/Backend Desync:** Role checks in the UI used lowercase strings (e.g., `Roles="superadmin"`) which relied on a brittle mapping. Nav links and action buttons were either completely unguarded or gated only by role rather than specific backend permissions (e.g., `menus.create`), breaking the principle of least privilege and ignoring backend rules.

### 7. Exact Fixes Applied
- **DI Loop Broken:** Refactored `UserContextService` to remove the `AuthenticationStateProvider` dependency. Converted its methods to synchronous. Updated `CustomAuthStateProvider` to actively push the resolved principal to `UserContextService.SetUser()`.
- **Semaphore Patched:** Added a 10-second timeout to `SemaphoreSlim` in `JwtAuthorizationHandler` and explicitly caught `OperationCanceledException` to guarantee `Release()`.
- **Auth Alignment:** Created frontend mirror constants (`AppPermissions.cs`, `AppRoles.cs`). Built a centralized `AuthorizationService.cs`. Replaced all `<AuthorizeView Roles="...">` with precise, backend-matching `<PermissionView Permission="...">` across layouts, nav menus, and all CRUD pages.

### 8. Current Understanding of the Most Likely Freeze Source
The primary freeze at `RunAsync()` is resolved. It was definitively caused by the `CustomAuthStateProvider` <-> `UserContextService` DI loop executing on the browser's single UI thread during the initial render tree construction. The secondary post-login freeze was caused by the `SemaphoreSlim` leak. Both are patched.

### 9. Startup Areas Left to Inspect
- **Initial Boot Edge Cases:** Potential `localStorage` JSInterop timing issues if the token is expired on the very first boot.
- **Network Timeouts:** The behavior of the application if the backend is completely unreachable during startup while `CustomAuthStateProvider` attempts token validation.

### 10. Current Status
**Status:** The Blazor WASM startup freeze and circular DI deadlocks have been successfully resolved. A comprehensive frontend-backend authorization alignment is complete, migrating the UI to strict, backend-matching permission guards.
- **Latest Work:** Extended the permission system to the `Sessions` module. Created new domain permissions (`sessions.view`, `sessions.create`, `sessions.update`, `sessions.delete`), seeded them to Admins via `DbSeeder`, and wrapped the `Sessions` navigation link and Revoke action buttons in `<PermissionView>`. Also removed legacy `.config` and `checkpoints` folders.
- **Next Steps:** The next engineer should focus on runtime testing in the browser (specifically initial load with an expired token or no network) to ensure no JS interop or network timeout hangs remain. No further architectural changes to DI or Authorization are expected.
