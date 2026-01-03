using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service de gestion des mises à jour système
/// </summary>
public interface IServiceMisesAJour
{
    /// <summary>
    /// Vérifie les mises à jour disponibles
    /// </summary>
    Task<UpdateInfo> VerifierMisesAJourAsync();
    
    /// <summary>
    /// Installe toutes les mises à jour disponibles
    /// </summary>
    /// <returns>True si l'installation a réussi, False sinon</returns>
    Task<(bool Success, string Message)> InstallerMisesAJourAsync(Action<string, string>? onLogReceived = null);
}