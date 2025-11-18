namespace Tuxpilot.Infrastructure.Dtos;

/// <summary>
/// DTO pour désérialiser le JSON du script Python
/// </summary>
public class UpdateInfoDto
{
    public string Gestionnaire { get; set; } = string.Empty;
    public int Nombre { get; set; }
    public List<PackageDto> Paquets { get; set; } = new();
    public string? Erreur { get; set; }
}