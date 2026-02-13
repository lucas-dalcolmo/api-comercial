# CRM API Examples

## Clients

### GET /api/crm/clients
Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Fortlev Demo Client",
        "legalName": "Fortlev Demo Client LTDA",
        "logoUrl": "/uploads/client-logos/demo-client.png",
        "active": true
      }
    ],
    "page": 1,
    "pageSize": 50,
    "totalCount": 1
  }
}
```

### GET /api/crm/clients/{id}
Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": {
    "id": 1,
    "name": "Fortlev Demo Client",
    "legalName": "Fortlev Demo Client LTDA",
    "logoUrl": "/uploads/client-logos/demo-client.png",
    "active": true
  }
}
```

### POST /api/crm/clients
Request:
```json
{
  "name": "Acme Brasil",
  "legalName": "Acme Brasil Servicos LTDA"
}
```
Response (201):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": {
    "id": 2,
    "name": "Acme Brasil",
    "legalName": "Acme Brasil Servicos LTDA",
    "logoUrl": null,
    "active": true
  }
}
```

### PATCH /api/crm/clients/{id}
Request:
```json
{
  "name": "Acme Brasil S/A",
  "active": true
}
```
Response (200): same shape of client DTO.

### POST /api/crm/clients/{id}/logo
Request: `multipart/form-data`, field `file` with `.png/.jpg/.jpeg/.webp` up to 2MB.

Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": {
    "id": 2,
    "name": "Acme Brasil S/A",
    "legalName": "Acme Brasil Servicos LTDA",
    "logoUrl": "/uploads/client-logos/client_2_2b66b28afcb0432b9e1fcb99431f6ea5.png",
    "active": true
  }
}
```

### DELETE /api/crm/clients/{id}/logo
Response (200): same shape of client DTO with `logoUrl: null`.

## Proposals

### GET /api/crm/proposals
Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": {
    "items": [
      {
        "id": 1,
        "clientId": 1,
        "clientName": "Fortlev Demo Client",
        "opportunityId": null,
        "title": "Proposta Comercial Demo",
        "objectiveHtml": "<p><strong>Objetivo:</strong> fornecer time dedicado com margem global de 20%.</p>",
        "globalMarginPercent": 20.0,
        "status": "Draft",
        "totalCost": 8300.0,
        "totalSellPrice": 9960.0,
        "active": true,
        "createdAt": "2026-02-13T19:30:00Z",
        "updatedAt": "2026-02-13T19:30:00Z"
      }
    ],
    "page": 1,
    "pageSize": 50,
    "totalCount": 1
  }
}
```

### GET /api/crm/proposals/{id}
Response (200): same shape of a proposal DTO.

### POST /api/crm/proposals
Request:
```json
{
  "clientId": 1,
  "opportunityId": 123,
  "title": "Bodyshop Squad 2026",
  "objectiveHtml": "<p><u>Objetivo:</u> alocar squad dedicado para sustentacao.</p>",
  "globalMarginPercent": 20.0,
  "status": "Draft"
}
```
Response (201): proposal DTO with totals initially zero.

### PATCH /api/crm/proposals/{id}
Request:
```json
{
  "globalMarginPercent": 25.0,
  "status": "Sent",
  "objectiveHtml": "<p><strong>Objetivo revisado</strong></p>"
}
```
Response (200): proposal DTO with recalculated totals.

### DELETE /api/crm/proposals/{id}
Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": true
}
```

### GET /api/crm/proposals/{id}/document/pdf
Response (200):
- Content-Type: `application/pdf`
- Content-Disposition: `attachment; filename="proposta-{id}.pdf"`
- Body: binary PDF
- Template source used by backend: `docs/templates/Modelo_proposta_comercial.docx`
- Placeholders used:
  - `[ClientName]`
  - `[Subject]`

Error 404 example:
```json
{
  "success": false,
  "message": "Proposal not found.",
  "error": "not_found",
  "traceId": "00-...",
  "data": null
}
```

Error 422 example (template/composition):
```json
{
  "success": false,
  "message": "Invalid template: logo placeholder image was not found.",
  "error": "domain_error",
  "traceId": "00-...",
  "data": null
}
```

## Proposal Employees

### GET /api/crm/proposals/{id}/employees
Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": [
    {
      "id": 1,
      "proposalId": 1,
      "employeeId": 10,
      "employeeName": "Alice Demo",
      "costSnapshot": 3300.0,
      "marginPercentApplied": 20.0,
      "sellPriceSnapshot": 3960.0,
      "active": true,
      "createdAt": "2026-02-13T19:35:00Z",
      "updatedAt": "2026-02-13T19:35:00Z"
    }
  ]
}
```

### POST /api/crm/proposals/{id}/employees
Request:
```json
{
  "employeeId": 10
}
```
Response (201): proposal employee DTO.

### DELETE /api/crm/proposals/{id}/employees/{proposalEmployeeId}
Response (200):
```json
{
  "success": true,
  "message": "Success",
  "error": null,
  "traceId": "00-...",
  "data": true
}
```

## Error examples

### 400 validation
```json
{
  "success": false,
  "message": "Title is required.",
  "error": "validation",
  "traceId": "00-...",
  "data": null
}
```

### 404 not found
```json
{
  "success": false,
  "message": "Record not found.",
  "error": "not_found",
  "traceId": "00-...",
  "data": null
}
```

### 409 conflict
```json
{
  "success": false,
  "message": "Employee already exists in this proposal.",
  "error": "conflict",
  "traceId": "00-...",
  "data": null
}
```

### 422 domain rule
```json
{
  "success": false,
  "message": "Cannot change margin for a closed proposal.",
  "error": "domain_error",
  "traceId": "00-...",
  "data": null
}
```
