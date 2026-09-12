using Microsoft.AspNetCore.Mvc;

namespace ColdChain.Gateway.Controllers;

[ApiController]
[Route("api/v1/tenants")]
public class TenantsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Tenant> Get() => db.Tenants;
}
