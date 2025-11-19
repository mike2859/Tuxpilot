namespace Tuxpilot.Infrastructure.Dtos;

public class LogEntryDto
{
    public string Timestamp { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}