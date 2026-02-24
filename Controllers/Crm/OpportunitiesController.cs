using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Crm;

[ApiController]
[Route("api/crm/opportunities")]
public sealed class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _service;

    public OpportunitiesController(IOpportunityService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OpportunityQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query.Active.HasValue ? query : query with { Active = true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OpportunityCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] OpportunityUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(id, dto, cancellationToken);
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
