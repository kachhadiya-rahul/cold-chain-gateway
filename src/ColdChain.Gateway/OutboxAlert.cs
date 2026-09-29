namespace ColdChain.Gateway;

public class OutboxAlert
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public double TemperatureC { get; set; }
    public double MaxTemperatureC { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
