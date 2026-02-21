using Microsoft.AspNetCore.Mvc;

namespace WAssis.Services.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { service = "WAssis.Services.Api" });
}
