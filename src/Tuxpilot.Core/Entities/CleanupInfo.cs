namespace Tuxpilot.Core.Entities;


/// <summary>
/// Informations sur le nettoyage système disponible
/// </summary>
public class CleanupInfo
{
    /// <summary>
    /// Gestionnaire de paquets utilisé
    /// </summary>
    public string Gestionnaire { get; set; } = string.Empty;
    
    /// <summary>
    /// Liste des éléments nettoyables
    /// </summary>
    public List<CleanupElement> Elements { get; set; } = new();
    
    /// <summary>
    /// Taille totale libérable en mégaoctets
    /// </summary>
    public long TailleTotaleMB { get; set; }
    
    /// <summary>
    /// Nombre d'éléments nettoyables
    /// </summary>
    public int NombreElements { get; set; }
    
    /// <summary>
    /// Message d'erreur si l'analyse a échoué
    /// </summary>
    public string? Erreur { get; set; }
    
    /// <summary>
    /// Taille totale en GB (calculée)
    /// </summary>
    public double TailleTotaleGB => TailleTotaleMB / 1024.0;
}