namespace Tuxpilot.Infrastructure.Dtos;

public class InstallResultDto
{
    public bool Success { get; set; }
    public string Gestionnaire { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}