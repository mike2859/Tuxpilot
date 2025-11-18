namespace Tuxpilot.Core.Entities;


/// <summary>
/// Représente un paquet disponible pour mise à jour
/// </summary>
public class Package
{
    /// <summary>
    /// Nom du paquet
    /// </summary>
    public string Nom { get; set; } = string.Empty;
    
    /// <summary>
    /// Version actuellement installée
    /// </summary>
    public string VersionActuelle { get; set; } = string.Empty;
    
    /// <summary>
    /// Version disponible pour mise à jour
    /// </summary>
    public string VersionDisponible { get; set; } = string.Empty;
    
    /// <summary>
    /// Dépôt d'où provient la mise à jour
    /// </summary>
    public string Depot { get; set; } = string.Empty;
}