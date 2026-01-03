using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service de gestion des services systemd
/// </summary>
public interface IServiceGestionServices
{
    /// <summary>
    /// Liste tous les services importants
    /// </summary>
    Task<List<ServiceSystemd>> ListerServicesAsync();
    
    /// <summary>
    /// Démarre un service
    /// </summary>
    Task<(bool Success, string Message)> DemarrerServiceAsync(string serviceName);
    
    /// <summary>
    /// Arrête un service
    /// </summary>
    Task<(bool Success, string Message)> ArreterServiceAsync(string serviceName);
    
    /// <summary>
    /// Redémarre un service
    /// </summary>
    Task<(bool Success, string Message)> RedemarrerServiceAsync(string serviceName);
    
    /// <summary>
    /// Récupère les logs d'un service
    /// </summary>
    Task<string> ObtenirLogsAsync(string serviceName, int lignes = 50);
}