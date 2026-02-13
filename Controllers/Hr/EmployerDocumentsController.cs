using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Hr;

[ApiController]
[Route("api/hr/employers/{employeeId:int}/documents")]
public sealed class EmployerDocumentsController : ControllerBase
{
    private readonly IEmployeeDocumentService _service;

    public EmployerDocumentsController(IEmployeeDocumentService service)
    {
        _service = service;
    }

    [HttpGet("{documentId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int employeeId, [FromRoute] int documentId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(employeeId, documentId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeDocumentQueryDto query, [FromRoute] int employeeId, CancellationToken cancellationToken)
    {
        var normalized = query with { EmployeeId = employeeId, Active = query.Active ?? true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int employeeId, [FromBody] EmployeeDocumentCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(employeeId, dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{documentId:int}")]
    public async Task<IActionResult> Patch([FromRoute] int employeeId, [FromRoute] int documentId, [FromBody] EmployeeDocumentUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(employeeId, documentId, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{documentId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int employeeId, [FromRoute] int documentId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(employeeId, documentId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{documentId:int}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] int employeeId, [FromRoute] int documentId, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(employeeId, documentId, cancellationToken);
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
            "not_found" => NotFound(errorPayload),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorPayload)
        };
    }
}
