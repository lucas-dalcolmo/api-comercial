# api-comercial

API comercial do ecossistema Apeiron. Projeto ASP.NET Core (.NET 10) com Swagger, autenticação e endpoints de domínio.

## Stack
- ASP.NET Core Web API (.NET 10)
- EF Core (SQL Server)
- Swagger (Swashbuckle)
- Microsoft Identity Web (Azure AD / Entra ID)

## Como rodar
1. Ajuste `appsettings.json` ou `appsettings.Development.json`.
2. Execute: `dotnet run`
3. Acesse o Swagger em `/swagger`.

## Configurações
No `appsettings.json`, configure:
- `ConnectionStrings:Default` com a string do SQL Server.
- `AzureAd:*` para autenticação em produção (Entra ID).
- `DevAuth:*` para autenticação simplificada em desenvolvimento.

Exemplo (placeholders):
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

## Autenticação
- **Development**: usa o esquema `DevBearer`.
  - Envie `Authorization: Bearer <token>` ou `?dev_token=<token>`.
  - O token esperado vem de `DevAuth:Token`.
- **Production**: usa Azure AD / Entra ID via `Microsoft.Identity.Web`.

## Endpoints atuais
- `GET /api/health` (healthcheck)
- `GET /api/whoami` (requer autenticação)
- Domínios (CRUD padrão):
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

## Scaffold DB-first (futuro)
Quando o banco estiver disponível, rode:

```bash
dotnet ef dbcontext scaffold "Server=SERVER;Database=DBNAME;User Id=USER;Password=PASSWORD;Encrypt=True;TrustServerCertificate=False;" Microsoft.EntityFrameworkCore.SqlServer --schema crm --output-dir Data/Entities --context-dir Data --context ApeironDbContext --use-database-names --no-pluralize --force
```

Observações:
- O `ApeironDbContext` atual é um placeholder parcial.
- Após o scaffold, mantenha/atualize o contexto gerado conforme necessário e remova o placeholder se houver conflito.
