using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ColdChain.Gateway;

namespace ColdChain.Gateway.Tests;

public class TenantTests
{
    [Fact]
    public async Task Get_returns_pharma_and_frozen()
    {
        await using var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();

        var tenants = await client.GetFromJsonAsync<List<Tenant>>("/api/v1/tenants");

        Assert.NotNull(tenants);
        Assert.Equal(2, tenants.Count);
        Assert.Contains(tenants, t => t.Id == "pharma" && t.MaxTemperatureC == 4);
        Assert.Contains(tenants, t => t.Id == "frozen" && t.MaxTemperatureC == 2);
    }
}
