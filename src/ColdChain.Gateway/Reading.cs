using System.ComponentModel.DataAnnotations;

namespace ColdChain.Gateway;

public class Reading
{
    [Required] public string TenantId { get; set; } = "";
    [Required] public string ReeferId { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TemperatureC { get; set; }
    public double Humidity { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}
