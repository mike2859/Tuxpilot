using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service de détection du système de planification
/// </summary>
public interface IServiceDetectionPlanificateur
{
    /// <summary>
    /// Détecter le planificateur disponible
    /// </summary>
    Task<TypePlanificateur> DetecterPlanificateurAsync();
    
    /// <summary>
    /// Vérifier si systemd est disponible
    /// </summary>
    Task<bool> SystemdDisponibleAsync();
    
    /// <summary>
    /// Vérifier si cron est disponible
    /// </summary>
    Task<bool> CronDisponibleAsync();
}