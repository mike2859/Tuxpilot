namespace Tuxpilot.Core.Entities;

/// <summary>
/// Représente une entrée de log système
/// </summary>
public class LogEntry
{
    public string Timestamp { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}