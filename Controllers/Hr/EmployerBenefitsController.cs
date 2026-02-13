using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Hr;

[ApiController]
[Route("api/hr/employers/{employeeId:int}/benefits")]
public sealed class EmployerBenefitsController : ControllerBase
{
    private readonly IEmployeeBenefitService _service;

    public EmployerBenefitsController(IEmployeeBenefitService service)
    {
        _service = service;
    }

    [HttpGet("{benefitId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int employeeId, [FromRoute] int benefitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(employeeId, benefitId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeBenefitQueryDto query, [FromRoute] int employeeId, CancellationToken cancellationToken)
    {
        var normalized = query with { EmployeeId = employeeId, Active = query.Active ?? true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int employeeId, [FromBody] EmployeeBenefitCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(employeeId, dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{benefitId:int}")]
    public async Task<IActionResult> Patch([FromRoute] int employeeId, [FromRoute] int benefitId, [FromBody] EmployeeBenefitUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(employeeId, benefitId, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{benefitId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int employeeId, [FromRoute] int benefitId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(employeeId, benefitId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{benefitId:int}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] int employeeId, [FromRoute] int benefitId, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(employeeId, benefitId, cancellationToken);
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
