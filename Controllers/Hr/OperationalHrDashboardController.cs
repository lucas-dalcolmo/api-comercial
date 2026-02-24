using Api.Comercial.Models.Dtos;
using Api.Comercial.Models.Responses;
using Api.Comercial.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Comercial.Controllers.Hr;

[ApiController]
[Route("api/hr/dashboard/operational")]
public sealed class OperationalHrDashboardController : ControllerBase
{
    private readonly IOperationalHrDashboardService _service;

    public OperationalHrDashboardController(IOperationalHrDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(OperationResult<T> result)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (result.Success)
        {
            var payload = ApiResponseFactory.Ok(result.Data!, traceId, "Success");
            return Ok(payload);
        }

        var errorCode = result.ErrorCode ?? "error";
        var errorMessage = result.ErrorMessage ?? "Unexpected error.";
        var errorPayload = ApiResponseFactory.Fail<T>(errorCode, errorMessage, traceId);
        return StatusCode(StatusCodes.Status500InternalServerError, errorPayload);
    }
}
