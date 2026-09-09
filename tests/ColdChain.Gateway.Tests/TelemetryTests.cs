using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ColdChain.Gateway;

namespace ColdChain.Gateway.Tests;

public class TelemetryTests
{
    [Fact]
    public async Task Post_returns_202()
    {
        await using var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/telemetry", Ping("R-1"));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Post_returns_503_when_full()
    {
        await using var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();

        HttpStatusCode last = 0;
        for (var i = 0; i < 257; i++)
        {
            last = (await client.PostAsJsonAsync("/api/v1/telemetry", Ping($"R-{i}"))).StatusCode;
        }

        Assert.Equal(HttpStatusCode.ServiceUnavailable, last);
    }

    static Reading Ping(string deviceId) => new()
    {
        TenantId = "pharma",
        DeviceId = deviceId,
        Latitude = 43.65,
        Longitude = -79.38,
        TemperatureC = -18.2,
        Humidity = 45,
        RecordedAt = DateTimeOffset.Parse("2026-09-07T20:00:00Z")
    };
}
