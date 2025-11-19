namespace Tuxpilot.Infrastructure.Dtos;

/// <summary>
/// DTO pour désérialiser le JSON du script cleanup.py
/// </summary>
public class CleanupInfoDto
{
    public string Gestionnaire { get; set; } = string.Empty;
    public List<CleanupElementDto> Elements { get; set; } = new();
    public long TailleTotaleMB { get; set; }
    public int NombreElements { get; set; }
    public string? Erreur { get; set; }
}