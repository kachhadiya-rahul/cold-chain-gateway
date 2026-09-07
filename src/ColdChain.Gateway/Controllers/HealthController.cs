using Microsoft.AspNetCore.Mvc;

namespace ColdChain.Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
