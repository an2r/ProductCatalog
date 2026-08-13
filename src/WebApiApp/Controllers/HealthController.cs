using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiApp.Data;

namespace WebApiApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { connected = false });
            }

            return Ok(new
            {
                connected = true,
                serverTimeUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                connected = false,
                error = ex.Message
            });
        }
    }
}
