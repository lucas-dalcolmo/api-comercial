using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Crm;

[ApiController]
[Route("api/crm/proposals")]
public sealed class ProposalsController : ControllerBase
{
    private readonly IProposalService _service;
    private readonly IProposalDocumentService _documentService;

    public ProposalsController(IProposalService service, IProposalDocumentService documentService)
    {
        _service = service;
        _documentService = documentService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProposalQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query.Active.HasValue ? query : query with { Active = true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProposalCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] ProposalUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(id, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Exports a proposal to PDF using the official DOCX template at docs/templates/Modelo_proposta_comercial.docx.
    /// </summary>
    /// <param name="id">Proposal identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Binary PDF file.</returns>
    [HttpGet("{id:int}/document/pdf")]
    [HttpPost("{id:int}/document/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportPdf([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _documentService.ExportPdfAsync(id, cancellationToken);
        if (result.Success && result.Data is not null)
        {
            return File(result.Data.Content, "application/pdf", result.Data.FileName);
        }

        var errorCode = result.ErrorCode ?? "error";
        var errorMessage = result.ErrorMessage ?? "Unexpected error.";
        var traceId = HttpContext.TraceIdentifier;
        var errorPayload = ApiResponseFactory.Fail<object>(errorCode, errorMessage, traceId);

        return errorCode switch
        {
            "not_found" => NotFound(errorPayload),
            "domain_error" => UnprocessableEntity(errorPayload),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorPayload)
        };
    }

    private IActionResult ToActionResult<T>(OperationResult<T> result, int? createdId = null)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.Success)
        {
            var payload = ApiResponseFactory.Ok(result.Data!, traceId, "Success");
            if (createdId.HasValue)
            {
                return Created($"{Request.Path}/{createdId.Value}", payload);
            }

            return Ok(payload);
        }

        var errorCode = result.ErrorCode ?? "error";
        var errorMessage = result.ErrorMessage ?? "Unexpected error.";
        var errorPayload = ApiResponseFactory.Fail<T>(errorCode, errorMessage, traceId);

        return errorCode switch
        {
            "validation" => BadRequest(errorPayload),
            "conflict" => Conflict(errorPayload),
            "domain_error" => UnprocessableEntity(errorPayload),
            "not_found" => NotFound(errorPayload),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorPayload)
        };
    }
}

[ApiController]
[Route("api/crm/proposals/{id:int}/employees")]
public sealed class ProposalEmployeesController : ControllerBase
{
    private readonly IProposalService _service;

    public ProposalEmployeesController(IProposalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetEmployeesAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int id, [FromBody] ProposalEmployeeAddDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.AddEmployeeAsync(id, dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpDelete("{proposalEmployeeId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromRoute] int proposalEmployeeId, CancellationToken cancellationToken)
    {
        var result = await _service.RemoveEmployeeAsync(id, proposalEmployeeId, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(OperationResult<T> result, int? createdId = null)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.Success)
        {
            var payload = ApiResponseFactory.Ok(result.Data!, traceId, "Success");
            if (createdId.HasValue)
            {
                return Created($"{Request.Path}/{createdId.Value}", payload);
            }

            return Ok(payload);
        }

        var errorCode = result.ErrorCode ?? "error";
        var errorMessage = result.ErrorMessage ?? "Unexpected error.";
        var errorPayload = ApiResponseFactory.Fail<T>(errorCode, errorMessage, traceId);

        return errorCode switch
        {
            "validation" => BadRequest(errorPayload),
            "conflict" => Conflict(errorPayload),
            "domain_error" => UnprocessableEntity(errorPayload),
            "not_found" => NotFound(errorPayload),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorPayload)
        };
    }
}
