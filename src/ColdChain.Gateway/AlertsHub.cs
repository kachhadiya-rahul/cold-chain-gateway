using Microsoft.AspNetCore.SignalR;

namespace ColdChain.Gateway;

public class AlertsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.GetHttpContext()?.Request.Query["tenantId"].ToString();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        await base.OnConnectedAsync();
    }
}
