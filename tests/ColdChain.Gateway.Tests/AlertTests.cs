using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using ColdChain.Gateway;

namespace ColdChain.Gateway.Tests;

public class AlertTests
{
    [Fact]
    public async Task Pharma_alert_does_not_reach_frozen()
    {
        await using var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();

        await using var pharma = Connect(app, "pharma");
        await using var frozen = Connect(app, "frozen");

        var got = new TaskCompletionSource();
        var frozenHits = 0;
        pharma.On<object>("alert", _ => got.TrySetResult());
        frozen.On<object>("alert", _ => Interlocked.Increment(ref frozenHits));

        await pharma.StartAsync();
        await frozen.StartAsync();

        var post = await client.PostAsJsonAsync("/api/v1/telemetry", new Reading
        {
            TenantId = "pharma",
            DeviceId = "R-100",
            TemperatureC = 5,
            RecordedAt = DateTimeOffset.UtcNow
        });
        Assert.Equal(HttpStatusCode.Accepted, post.StatusCode);

        await got.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(200);
        Assert.Equal(0, frozenHits);
    }

    static HubConnection Connect(WebApplicationFactory<Program> app, string tenant) =>
        new HubConnectionBuilder()
            .WithUrl(new Uri(app.Server.BaseAddress!, $"/hubs/alerts?tenantId={tenant}"), o =>
            {
                o.HttpMessageHandlerFactory = _ => app.Server.CreateHandler();
            })
            .Build();
}
