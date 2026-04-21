# Menu

Menu is a multi-project solution for restaurant menu management.

## Solution Structure

- `Menu.Api` - ASP.NET Core Web API
- `Menu.Application` - Application layer (services, DTOs, validators)
- `Menu.Domain` - Domain entities and interfaces
- `Menu.Infrastructure` - Data access, repositories, infrastructure services
- `Menu.UI` - Blazor UI application

## Prerequisites

- .NET SDK (version required by the solution)
- SQL Server (or configured database provider)

## Run Locally

1. Restore dependencies:
   `dotnet restore`
2. Build solution:
   `dotnet build`
3. Run API:
   `dotnet run --project Menu.Api`
4. Run UI:
   `dotnet run --project Menu.UI`

## Initial Git Setup

If this repository is not initialized yet, run:

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/abdokhafagy/Menu.git
git push -u origin main
```
