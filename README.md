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
  - `api/domains/nationalities`
  - `api/domains/countries`
  - `api/domains/currencies`
- HR:
  - `api/hr/employers`
  - `api/hr/employer-contracts`
  - `api/hr/employers/{id}/documents`
  - `api/hr/employers/{id}/benefits`
- Commercial configuration:
  - `api/configuration/commercial/score-category-weights`
  - `api/configuration/commercial/score-value-weights`
- CRM Proposals:
  - `GET /api/crm/clients`
  - `GET /api/crm/clients/{id}`
  - `POST /api/crm/clients`
  - `PATCH /api/crm/clients/{id}`
  - `POST /api/crm/clients/{id}/logo` (`multipart/form-data`, field `file`, max 2MB, png/jpg/webp)
  - `DELETE /api/crm/clients/{id}/logo`
  - `GET /api/crm/proposals`
  - `GET /api/crm/proposals/{id}`
  - `POST /api/crm/proposals`
  - `PATCH /api/crm/proposals/{id}`
  - `DELETE /api/crm/proposals/{id}`
  - `GET /api/crm/proposals/{id}/document/pdf` (official endpoint for proposal export)
  - `GET /api/crm/proposals/{id}/employees`
  - `POST /api/crm/proposals/{id}/employees`
  - `DELETE /api/crm/proposals/{id}/employees/{proposalEmployeeId}`
- CRM Opportunities:
  - `GET /api/crm/opportunities`
  - `GET /api/crm/opportunities/{id}`
  - `POST /api/crm/opportunities`
  - `PATCH /api/crm/opportunities/{id}`
- Dashboards:
  - `GET /api/crm/dashboard/commercial`
  - `GET /api/hr/dashboard/operational`

Detailed payload/response examples: `docs-crm-examples.md`.

## Proposal PDF export
- Official endpoint: `GET /api/crm/proposals/{id}/document/pdf`
- Compatible verb also accepted: `POST /api/crm/proposals/{id}/document/pdf`
- Official template source: `docs/templates/Modelo_proposta_comercial.docx`
- Template placeholders:
  - `[ClientName]` -> client name
  - `[Subject]` -> proposal `objectiveHtml` (basic rich text mapped to DOCX)
  - client logo replaces the image nearest to the first `[ClientName]` in the same document part
- TOC/Summary refresh:
  - fields and table of contents are refreshed during DOCX->PDF conversion (with repagination) to keep summary/page numbers consistent after objective content changes
- Logo sizing rules in PDF pipeline:
  - rectangular logo: fit up to `5cm x 3cm` (practically `5cm x Y` with proportional height)
  - square/square-like logo: fit up to `3cm x 3cm`
  - keep aspect ratio (`contain`), no stretch/distortion
  - preserve template text/paragraph alignment (only logo image geometry is changed)
- Response:
  - `200` with `application/pdf`
  - `Content-Disposition: attachment; filename="proposta-{id}.pdf"`
- Error mapping:
  - `404` proposal not found
  - `422` template/composition/conversion errors
  - `500` unexpected errors

## Proposal pricing fields
- Proposal contract includes:
  - `projectHours` (`> 0`, validation on create/update)
  - `globalMarginPercent`
  - `totalCost`
  - `totalSellPrice`
- Proposal employee contract includes:
  - `id`
  - `proposalEmployeeId`
  - `proposalId`
  - `employeeId`
  - `employeeName`
  - `costSnapshot`
  - `marginPercentApplied`
  - `sellPriceSnapshot`
  - `hourlyValueSnapshot`
  - `active`
  - `createdAt`
  - `updatedAt`
- Hourly value rule:
  - `hourlyValueSnapshot = sellPriceSnapshot / 220` (rounded to 4 decimals)

## Opportunity flow and probability
- Commercial lifecycle is enforced as:
  - create/select client
  - create opportunity
  - create proposal linked to opportunity
- Proposal rules:
  - `OpportunityId` is required on proposal creation
  - opportunity must belong to selected client
  - only one active proposal per opportunity
  - linked `OpportunityId` cannot be changed after proposal creation
- Probability:
  - calculated in backend from weighted commercial factors
  - recalculated and persisted on create/update of opportunity
  - exposed in opportunity list and detail DTOs (`probabilityPercent`)

## Logo upload and persistence
- Upload endpoint uses `multipart/form-data` DTO wrapping (`ClientLogoUploadDto`) for Swagger compatibility with `IFormFile`.
- Client logo path is persisted in `crm.Client.LogoUrl`.
- Proposal PDF export requires a persisted client logo (`domain_error` when missing).

## Dashboards
- Commercial dashboard (`/api/crm/dashboard/commercial`) uses current CRM data and returns:
  - KPI cards
  - recent opportunities
  - weighted forecast by quarter
  - weighted forecast by semester
- Operational HR dashboard (`/api/hr/dashboard/operational`) returns operational KPIs and allocation-focused data model for frontend widgets.

## Proposal Employees stability/performance
- Endpoint: `GET /api/crm/proposals/{id}/employees`
- Query behavior:
  - translated SQL ordering by employee name before DTO projection
  - no client-side evaluation required for sorting
  - no N+1 pattern in listing
- Response behavior:
  - `200` with empty list when proposal has no active linked employees

## Scaffold DB-first (future)
When the database is available, run:

```bash
dotnet ef dbcontext scaffold "Server=SERVER;Database=DBNAME;User Id=USER;Password=PASSWORD;Encrypt=True;TrustServerCertificate=False;" Microsoft.EntityFrameworkCore.SqlServer --schema crm --output-dir Data/Entities --context-dir Data --context ApeironDbContext --use-database-names --no-pluralize --force
```

Notes:
- The current `ApeironDbContext` is a partial placeholder.
- After scaffolding, keep/update the generated context as needed and remove the placeholder if it conflicts.

## DB scripts and migrations
- Base bootstrap: `scripts/init-db.sql`
- CRM Proposals/Clients migration: `scripts/migrations/20260213_crm_proposals_clients.sql`
- CRM Opportunities migration/backfill: `scripts/migrations/20260224_crm_opportunities.sql`
