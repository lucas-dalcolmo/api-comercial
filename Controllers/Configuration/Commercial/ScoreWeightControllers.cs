using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Configuration.Commercial;

[ApiController]
[Route("api/configuration/commercial/score-category-weights")]
public sealed class ScoreCategoryWeightsController : ControllerBase
{
    private readonly IScoreCategoryWeightService _service;

    public ScoreCategoryWeightsController(IScoreCategoryWeightService service)
    {
        _service = service;
    }

    [HttpGet("{categoryName}")]
    public async Task<IActionResult> GetById([FromRoute] string categoryName, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(categoryName, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ScoreCategoryWeightQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query.Active.HasValue ? query : query with { Active = true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScoreCategoryWeightCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.CategoryName);
    }

    [HttpPatch("{categoryName}")]
    public async Task<IActionResult> Patch([FromRoute] string categoryName, [FromBody] ScoreCategoryWeightUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(categoryName, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{categoryName}")]
    public async Task<IActionResult> Delete([FromRoute] string categoryName, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(categoryName, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{categoryName}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] string categoryName, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(categoryName, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(OperationResult<T> result, string? createdId = null)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.Success)
        {
            var payload = ApiResponseFactory.Ok(result.Data!, traceId, "Success");
            if (!string.IsNullOrWhiteSpace(createdId))
            {
                return Created($"{Request.Path}/{createdId}", payload);
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

[ApiController]
[Route("api/configuration/commercial/score-value-weights")]
public sealed class ScoreValueWeightsController : ControllerBase
{
    private readonly IScoreValueWeightService _service;

    public ScoreValueWeightsController(IScoreValueWeightService service)
    {
        _service = service;
    }

    [HttpGet("{categoryName}/{valueName}")]
    public async Task<IActionResult> GetById([FromRoute] string categoryName, [FromRoute] string valueName, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(categoryName, valueName, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ScoreValueWeightQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query.Active.HasValue ? query : query with { Active = true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScoreValueWeightCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return ToActionResult(result, createdId: $"{result.Data?.CategoryName}/{result.Data?.ValueName}");
    }

    [HttpPatch("{categoryName}/{valueName}")]
    public async Task<IActionResult> Patch([FromRoute] string categoryName, [FromRoute] string valueName, [FromBody] ScoreValueWeightUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(categoryName, valueName, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{categoryName}/{valueName}")]
    public async Task<IActionResult> Delete([FromRoute] string categoryName, [FromRoute] string valueName, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(categoryName, valueName, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{categoryName}/{valueName}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] string categoryName, [FromRoute] string valueName, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(categoryName, valueName, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(OperationResult<T> result, string? createdId = null)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.Success)
        {
            var payload = ApiResponseFactory.Ok(result.Data!, traceId, "Success");
            if (!string.IsNullOrWhiteSpace(createdId))
            {
                return Created($"{Request.Path}/{createdId}", payload);
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
