using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Api.Comercial.Data;

namespace Api.Comercial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApeironDbContext _db;

    public HealthController(ApeironDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var sample = await _db.BloodTypes
                .AsNoTracking()
                .Select(b => new { b.Id, b.Name })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                status = "ok",
                service = "api-comercial",
                dbConnected = true,
                bloodTypeSample = sample,
                db = GetDbInfo()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "error",
                service = "api-comercial",
                dbConnected = false,
                error = ex.Message,
                db = GetDbInfo()
            });
        }
    }

    private object? GetDbInfo()
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return null;
        }

        var cs = _db.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            return null;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(cs);
            return new
            {
                dataSource = builder.DataSource,
                database = builder.InitialCatalog,
                integratedSecurity = builder.IntegratedSecurity,
                userId = builder.UserID
            };
        }
        catch
        {
            return null;
        }
    }
}
