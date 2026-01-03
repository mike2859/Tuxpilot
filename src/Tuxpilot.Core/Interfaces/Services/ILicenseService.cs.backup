using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;

/// <summary>
/// Service de gestion des licences
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Vérifie la licence actuellement stockée (offline)
    /// </summary>
    Task<License> GetCurrentLicenseAsync();
        
    /// <summary>
    /// Active une nouvelle licence avec la clé fournie (appelle l'API)
    /// </summary>
    Task<License> ActivateLicenseAsync(string licenseKey);
        
    /// <summary>
    /// Révoque la licence actuelle (retour à Community)
    /// </summary>
    Task<bool> RevokeLicenseAsync();
        
    /// <summary>
    /// Vérifie si une fonctionnalité est disponible
    /// </summary>
    Task<bool> HasFeatureAsync(string feature);
        
    /// <summary>
    /// Obtient la liste des fonctionnalités disponibles
    /// </summary>
    Task<List<string>> GetAvailableFeaturesAsync();
}