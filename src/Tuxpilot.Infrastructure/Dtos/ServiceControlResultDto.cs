namespace Tuxpilot.Infrastructure.Dtos;

public class ServiceControlResultDto
{
    public bool Success { get; set; }
    public string Service { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}