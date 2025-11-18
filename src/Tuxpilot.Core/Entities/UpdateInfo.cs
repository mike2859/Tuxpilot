namespace Tuxpilot.Core.Entities;


/// <summary>
/// Informations sur les mises à jour disponibles
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// Gestionnaire de paquets utilisé
    /// </summary>
    public string Gestionnaire { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de mises à jour disponibles
    /// </summary>
    public int Nombre { get; set; }
    
    /// <summary>
    /// Liste des paquets à mettre à jour
    /// </summary>
    public List<Package> Paquets { get; set; } = new();
    
    /// <summary>
    /// Message d'erreur si la vérification a échoué
    /// </summary>
    public string? Erreur { get; set; }
    
    /// <summary>
    /// Indique si des mises à jour sont disponibles
    /// </summary>
    public bool MisesAJourDisponibles => Nombre > 0;
}