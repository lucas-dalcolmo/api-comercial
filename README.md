# api-comercial

Commercial API for the Apeiron ecosystem. ASP.NET Core (.NET 10) project with Swagger, authentication, and domain endpoints.

## Stack
- ASP.NET Core Web API (.NET 10)
- EF Core (SQL Server)
- Swagger (Swashbuckle)
- Microsoft Identity Web (Azure AD / Entra ID)

## How to run
1. Update `appsettings.json` or `appsettings.Development.json`.
2. Run: `dotnet run`
3. Open Swagger at `/swagger`.

## Configuration
In `appsettings.json`, set:
- `ConnectionStrings:Default` with your SQL Server connection string.
- `AzureAd:*` for production authentication (Entra ID).
- `DevAuth:*` for simplified development authentication.

Example (placeholders):
```json
{
  "ConnectionStrings": {
    "Default": "Server=SERVER;Database=DBNAME;User Id=USER;Password=PASSWORD;Encrypt=True;TrustServerCertificate=False;"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "Domain": "yourtenant.onmicrosoft.com"
  },
  "DevAuth": {
    "Token": "dev-token",
    "Name": "Dev User",
    "PreferredUsername": "dev.user@local",
    "Oid": "00000000-0000-0000-0000-000000000000",
    "Sub": "00000000-0000-0000-0000-000000000000",
    "TenantId": "dev-tenant"
  }
}
```

## Authentication
- **Development**: uses the `DevBearer` scheme.
  - Send `Authorization: Bearer <token>` or `?dev_token=<token>`.
  - The expected token comes from `DevAuth:Token`.
- **Production**: uses Azure AD / Entra ID via `Microsoft.Identity.Web`.

## Current endpoints
- `GET /api/health` (health check)
- `GET /api/whoami` (requires authentication)
- Domains (standard CRUD):
  - `api/domains/states`
  - `api/domains/blood-types`
  - `api/domains/budget-levels`
  - `api/domains/company-sizes`
  - `api/domains/departments`
  - `api/domains/education-levels`
  - `api/domains/employment-types`
  - `api/domains/funnel-stages`
  - `api/domains/genders`
  - `api/domains/interaction-types`
  - `api/domains/lead-sources`
  - `api/domains/marital-statuses`
  - `api/domains/offices`
  - `api/domains/opportunity-statuses`
  - `api/domains/regions`
  - `api/domains/relationship-levels`
  - `api/domains/roles`
  - `api/domains/segments`
  - `api/domains/service-types`
  - `api/domains/technical-fits`
  - `api/domains/urgency-levels`
  - `api/domains/countries`
  - `api/domains/currencies`

## Scaffold DB-first (future)
When the database is available, run:

```bash
dotnet ef dbcontext scaffold "Server=SERVER;Database=DBNAME;User Id=USER;Password=PASSWORD;Encrypt=True;TrustServerCertificate=False;" Microsoft.EntityFrameworkCore.SqlServer --schema crm --output-dir Data/Entities --context-dir Data --context ApeironDbContext --use-database-names --no-pluralize --force
```

Notes:
- The current `ApeironDbContext` is a partial placeholder.
- After scaffolding, keep/update the generated context as needed and remove the placeholder if it conflicts.