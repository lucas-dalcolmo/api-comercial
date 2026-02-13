using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Hr;

[ApiController]
[Route("api/hr/employer-contracts/{contractId:int}/benefits")]
public sealed class EmployerContractBenefitsController : ControllerBase
{
    private readonly IEmployeeContractBenefitService _service;

    public EmployerContractBenefitsController(IEmployeeContractBenefitService service)
    {
        _service = service;
    }

    [HttpGet("{benefitId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int contractId, [FromRoute] int benefitId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(contractId, benefitId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeContractBenefitQueryDto query, [FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var normalized = query with { ContractId = contractId, Active = query.Active ?? true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int contractId, [FromBody] EmployeeContractBenefitCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(contractId, dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{benefitId:int}")]
    public async Task<IActionResult> Patch([FromRoute] int contractId, [FromRoute] int benefitId, [FromBody] EmployeeContractBenefitUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(contractId, benefitId, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{benefitId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int contractId, [FromRoute] int benefitId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(contractId, benefitId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{benefitId:int}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] int contractId, [FromRoute] int benefitId, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(contractId, benefitId, cancellationToken);
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
