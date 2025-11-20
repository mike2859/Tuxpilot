namespace Tuxpilot.Infrastructure.Dtos;

public class ServiceSystemdDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Enabled { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}