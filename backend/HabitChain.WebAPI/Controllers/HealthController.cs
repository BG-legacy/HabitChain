using Microsoft.AspNetCore.Mvc;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly HabitChainDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HabitChainDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            // Basic health check - just return OK
            var response = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new { status = "unhealthy", error = ex.Message });
        }
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            var response = new
            {
                status = canConnect ? "healthy" : "unhealthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                services = new
                {
                    database = canConnect ? "connected" : "disconnected"
                }
            };

            return canConnect ? Ok(response) : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return StatusCode(500, new { status = "unhealthy", error = ex.Message });
        }
    }
} 