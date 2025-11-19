namespace Tuxpilot.Core.Entities;

/// <summary>
/// Représente les informations du disque
/// </summary>
public class DiskInfo
{
    public string Partition { get; set; } = string.Empty;
    public string Taille { get; set; } = string.Empty;
    public string Utilise { get; set; } = string.Empty;
    public string Disponible { get; set; } = string.Empty;
    public int Pourcentage { get; set; }
}