using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Factory pour créer le bon service de planification selon le système disponible
/// </summary>
public class ServicePlanificationFactory
{
    private readonly ExecuteurScriptPython _executeur;
    private readonly IServiceDetectionPlanificateur _serviceDetection;
    private IServicePlanification? _serviceCache;
    private TypePlanificateur? _typePlanificateurCache;

    public ServicePlanificationFactory(
        ExecuteurScriptPython executeur,
        IServiceDetectionPlanificateur serviceDetection)
    {
        _executeur = executeur;
        _serviceDetection = serviceDetection;
    }

    /// <summary>
    /// Obtenir le service de planification approprié
    /// </summary>
    public async Task<IServicePlanification> ObtenirServiceAsync()
    {
        // Utiliser le cache si déjà créé
        if (_serviceCache != null && _typePlanificateurCache.HasValue)
        {
            Console.WriteLine($"[FACTORY] Utilisation cache: {_typePlanificateurCache.Value}");
            return _serviceCache;
        }

        // Détecter le planificateur disponible
        var typePlanificateur = await _serviceDetection.DetecterPlanificateurAsync();
        _typePlanificateurCache = typePlanificateur;

        // Créer le service approprié
        _serviceCache = typePlanificateur switch
        {
            TypePlanificateur.Systemd => new ServicePlanificationSystemd(_executeur),
            TypePlanificateur.Cron => new ServicePlanificationCron(_executeur),
            _ => throw new Exception("Aucun système de planification disponible. Veuillez installer systemd ou cron.")
        };

        Console.WriteLine($"[FACTORY] Service créé: {typePlanificateur}");
        return _serviceCache;
    }

    /// <summary>
    /// Forcer la re-détection (utile après installation de cron par exemple)
    /// </summary>
    public void ResetCache()
    {
        _serviceCache = null;
        _typePlanificateurCache = null;
        Console.WriteLine("[FACTORY] Cache réinitialisé");
    }

    /// <summary>
    /// Obtenir le type de planificateur actuellement utilisé
    /// </summary>
    public async Task<TypePlanificateur> ObtenirTypePlanificateurAsync()
    {
        if (_typePlanificateurCache.HasValue)
        {
            return _typePlanificateurCache.Value;
        }

        return await _serviceDetection.DetecterPlanificateurAsync();
    }
}