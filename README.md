# Menu

Menu is a full-stack restaurant menu management system built with ASP.NET Core Web API and Blazor WebAssembly.

It includes:
- Authentication and JWT-based authorization
- Role and permission management
- Restaurant, menu, category, and item management
- Item options and option values (single and multiple selection)
- Public menu browsing endpoints
- File upload support for images

## Architecture

The solution follows a clean, layered architecture:

- Menu.Api
   Hosts REST endpoints, middleware, authentication, authorization, and Swagger.
- Menu.Application
   Contains business logic, DTOs, validators, mapping profiles, and service interfaces.
- Menu.Domain
   Defines core entities, enums, and repository abstractions.
- Menu.Infrastructure
   Implements EF Core DbContext, repositories, token service, public menu service, migrations, and seeding.
- Menu.UI
   Blazor WebAssembly frontend with MudBlazor UI components and typed service clients.

## Tech Stack

- .NET (ASP.NET Core + Blazor WebAssembly)
- Entity Framework Core (SQL Server provider)
- JWT authentication
- FluentValidation
- AutoMapper
- MudBlazor
- Blazored.LocalStorage

## Project Structure

Key folders:

- Menu.Api/Controllers
   API endpoints for auth, users, roles, permissions, restaurants, menus, categories, items, options, values, files, and public browsing.
- Menu.Api/Middleware
   Global exception handling and session validation.
- Menu.Application/Services
   Application-level CRUD and business services.
- Menu.Application/Validators
   Request validation rules.
- Menu.Infrastructure/Data
   DbContext, entity configurations, and database seeding.
- Menu.Infrastructure/Migrations
   EF Core schema migrations.
- Menu.UI/Pages
   Blazor pages for admin and public menu experiences.

## Prerequisites

- .NET SDK 8.0 or later
- SQL Server instance (local or hosted)
- Git

## Local Development

### 1. Restore and build

```bash
dotnet restore
dotnet build
```

### 2. Configure API settings

Edit Menu.Api/appsettings.Development.json:

- ConnectionStrings:DefaultConnection
- Jwt:Issuer
- Jwt:Audience
- Jwt:Key

Recommended:
- Use a strong JWT key (at least 32 characters).
- Store secrets outside source control for production.

### 3. Run backend API

```bash
dotnet run --project Menu.Api
```

Default local URLs (from launch settings):
- https://localhost:7106
- http://localhost:5092

Swagger UI:
- https://localhost:7106/swagger

### 4. Run frontend UI

In a second terminal:

```bash
dotnet run --project Menu.UI
```

Default local URLs (from launch settings):
- https://localhost:7173
- http://localhost:5173

### 5. API base URL for UI

Menu.UI uses:
- Development: https://localhost:7106/
- Production fallback: value from Menu.UI/wwwroot/appsettings.json (ApiBaseUrl)

## Database and Migrations

The API automatically applies pending EF Core migrations on startup and then runs seeding.

Useful commands:

```bash
dotnet ef migrations add <MigrationName> --project Menu.Infrastructure --startup-project Menu.Api
dotnet ef database update --project Menu.Infrastructure --startup-project Menu.Api
```

## Seeded Data

On first run (when roles are not present), seeding creates:
- Roles: SuperAdmin, Admin, Manager, User
- Required permissions across modules
- Demo Restaurant and sample menu data
- Default admin user:
   - Username: admin
   - Password: Admin@123

Important:
- Change default credentials immediately in non-demo environments.

## Authentication and Authorization

- JWT Bearer authentication is enabled in the API.
- Session-based validation middleware checks active sessions.
- Permission-based protection is used via RequirePermission attributes on endpoints.

Auth endpoints include:
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/logout
- POST /api/auth/logout-all
- GET /api/auth/sessions
- POST /api/auth/sessions/{sessionId}/revoke

## API Surface Overview

Main controller groups:
- /api/restaurants
- /api/menus
- /api/categories
- /api/menuitems
- /api/itemoptions
- /api/optionvalues
- /api/users
- /api/roles
- /api/permissions
- /api/files/images
- /api/public/* (public browsing/search)

Use Swagger for full request/response contracts.

## Static Files and Uploads

- API serves static files from Menu.Api/wwwroot.
- Uploaded images are handled by FilesController.

## Deployment Notes

- Ensure production appsettings values are environment-specific.
- Do not keep production connection strings or JWT secrets in committed files.
- Enable HTTPS and secure CORS policy for production clients.

## Git Ignore

The repository includes a .gitignore configured for:
- build outputs (bin, obj)
- IDE files (.vs, .idea, .vscode)
- logs, test outputs, and local environment files

## Repository Setup

If creating a fresh remote setup:

```bash
git init
git add .
git commit -m "first commit"
git branch -M main
git remote add origin https://github.com/abdokhafagy/Menu.git
git push -u origin main
```
