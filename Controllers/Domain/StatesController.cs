using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Domain;

[ApiController]
[Route("api/domains/states")]
public sealed class StatesController : ControllerBase
{
    private readonly IStateService _service;

    public StatesController(IStateService service)
    {
        _service = service;
    }

    [HttpGet("{stateCode}")]
    public async Task<IActionResult> GetById([FromRoute] string stateCode, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(stateCode, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StateQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query.Ativo.HasValue ? query : query with { Ativo = true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StateCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return ToActionResult(result, createdCode: result.Data?.StateCode);
    }

    [HttpPatch("{stateCode}")]
    public async Task<IActionResult> Patch([FromRoute] string stateCode, [FromBody] StateUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(stateCode, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{stateCode}")]
    public async Task<IActionResult> Delete([FromRoute] string stateCode, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(stateCode, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{stateCode}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] string stateCode, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(stateCode, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(OperationResult<T> result, string? createdCode = null)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.Success)
        {
            var payload = ApiResponseFactory.Ok(result.Data!, traceId, "Success");
            if (!string.IsNullOrWhiteSpace(createdCode))
            {
                return Created($"{Request.Path}/{createdCode}", payload);
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
