using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service de gestion de la planification des tâches
/// </summary>
public interface IServicePlanification
{
    /// <summary>
    /// Obtenir toutes les tâches planifiées
    /// </summary>
    Task<List<TachePlanifiee>> ObtenirTachesAsync();
    
    /// <summary>
    /// Ajouter une tâche planifiée
    /// </summary>
    Task<bool> AjouterTacheAsync(TachePlanifiee tache);
    
    /// <summary>
    /// Supprimer une tâche planifiée
    /// </summary>
    Task<bool> SupprimerTacheAsync(string id);
    
    /// <summary>
    /// Activer/Désactiver une tâche
    /// </summary>
    Task<bool> ToggleTacheAsync(string id);
    
    /// <summary>
    /// Obtenir une tâche par ID
    /// </summary>
    Task<TachePlanifiee?> ObtenirTacheAsync(string id);
    
    /// <summary>
    /// Mettre à jour une tâche
    /// </summary>
    Task<bool> MettreAJourTacheAsync(TachePlanifiee tache);
}