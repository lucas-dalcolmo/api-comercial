using Microsoft.AspNetCore.Mvc;
using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;

namespace Api.Comercial.Controllers.Hr;

[ApiController]
[Route("api/hr/employers/{employeeId:int}/contracts")]
public sealed class EmployerContractsController : ControllerBase
{
    private readonly IEmployeeContractService _service;

    public EmployerContractsController(IEmployeeContractService service)
    {
        _service = service;
    }

    [HttpGet("{contractId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int employeeId, [FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(employeeId, contractId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeContractQueryDto query, [FromRoute] int employeeId, CancellationToken cancellationToken)
    {
        var normalized = query with { EmployeeId = employeeId, Active = query.Active ?? true };
        var result = await _service.GetAllAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int employeeId, [FromBody] EmployeeContractCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(employeeId, dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{contractId:int}")]
    public async Task<IActionResult> Patch([FromRoute] int employeeId, [FromRoute] int contractId, [FromBody] EmployeeContractUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchAsync(employeeId, contractId, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{contractId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int employeeId, [FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(employeeId, contractId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{contractId:int}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] int employeeId, [FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateAsync(employeeId, contractId, cancellationToken);
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

[ApiController]
[Route("api/hr/employer-contracts")]
public sealed class EmployerContractsGlobalController : ControllerBase
{
    private readonly IEmployeeContractService _service;

    public EmployerContractsGlobalController(IEmployeeContractService service)
    {
        _service = service;
    }

    [HttpGet("{contractId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdGlobalAsync(contractId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeContractQueryDto query, CancellationToken cancellationToken)
    {
        var normalized = query with { EmployeeId = null, Active = query.Active ?? true };
        var result = await _service.GetAllGlobalAsync(normalized, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeContractCreateGlobalDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateGlobalAsync(dto, cancellationToken);
        return ToActionResult(result, createdId: result.Data?.Id);
    }

    [HttpPatch("{contractId:int}")]
    public async Task<IActionResult> Patch([FromRoute] int contractId, [FromBody] EmployeeContractUpdateGlobalDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.PatchGlobalAsync(contractId, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{contractId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteGlobalAsync(contractId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{contractId:int}/reactivate")]
    public async Task<IActionResult> Reactivate([FromRoute] int contractId, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivateGlobalAsync(contractId, cancellationToken);
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
