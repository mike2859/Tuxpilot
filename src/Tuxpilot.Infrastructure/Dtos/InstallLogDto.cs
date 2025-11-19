namespace Tuxpilot.Infrastructure.Dtos;

/// <summary>
/// DTO pour un message de log d'installation
/// </summary>
public class InstallLogDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}