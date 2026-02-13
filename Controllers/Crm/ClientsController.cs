using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Crm;

[ApiController]
[Route("api/crm/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _service;

    public ClientsController(IClientService service)
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
    public async Task<IActionResult> GetAll([FromQuery] ClientQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query.Active.HasValue ? query : query with { Active = true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] ClientUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(id, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/logo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo([FromRoute] int id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var result = await _service.UploadLogoAsync(id, file, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}/logo")]
    public async Task<IActionResult> RemoveLogo([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _service.RemoveLogoAsync(id, cancellationToken);
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
