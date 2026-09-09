using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace ColdChain.Gateway.Controllers;

[ApiController]
[Route("api/v1/telemetry")]
public class TelemetryController(ChannelWriter<Reading> queue) : ControllerBase
{
    [HttpPost]
    public IActionResult Post(Reading reading)
    {
        if (!queue.TryWrite(reading))
            return StatusCode(503);

        return Accepted();
    }
}
