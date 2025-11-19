using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;

/// <summary>
/// Service de nettoyage système
/// </summary>
public interface IServiceNettoyage
{
    /// <summary>
    /// Analyse les éléments nettoyables du système
    /// </summary>
    Task<CleanupInfo> AnalyserNettoyageAsync();
}