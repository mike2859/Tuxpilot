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
}