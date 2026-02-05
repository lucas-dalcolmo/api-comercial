using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Comercial.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WhoAmIController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name");
        var preferredUsername = User.FindFirstValue("preferred_username");
        var oid = User.FindFirstValue("oid");
        var sub = User.FindFirstValue("sub");
        var tenantId = User.FindFirstValue("tid");

        return Ok(new
        {
            name,
            preferred_username = preferredUsername,
            oid,
            sub,
            tenant_id = tenantId
        });
    }
}
