using System.Text.Json;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Service de détection du planificateur
/// </summary>
public class ServiceDetectionPlanificateur : IServiceDetectionPlanificateur
{
    private readonly ExecuteurScriptPython _executeur;
    private TypePlanificateur? _planificateurCache;

    public ServiceDetectionPlanificateur(ExecuteurScriptPython executeur)
    {
        _executeur = executeur;
    }

    /// <summary>
    /// Détecter le planificateur disponible
    /// </summary>
    public async Task<TypePlanificateur> DetecterPlanificateurAsync()
    {
        // Utiliser le cache si déjà détecté
        if (_planificateurCache.HasValue)
        {
            return _planificateurCache.Value;
        }

        try
        {
            var jsonResult = await _executeur.ExecuterAsync("detecter_planificateur.py");

            if (string.IsNullOrEmpty(jsonResult))
            {
                Console.WriteLine("[DÉTECTION] Aucun résultat");
                _planificateurCache = TypePlanificateur.Aucun;
                return TypePlanificateur.Aucun;
            }

            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            if (root.TryGetProperty("recommande", out var recommandeElement))
            {
                var recommande = recommandeElement.GetString();
                _planificateurCache = recommande switch
                {
                    "systemd" => TypePlanificateur.Systemd,
                    "cron" => TypePlanificateur.Cron,
                    _ => TypePlanificateur.Aucun
                };

                Console.WriteLine($"[DÉTECTION] Planificateur recommandé : {_planificateurCache}");
                return _planificateurCache.Value;
            }

            _planificateurCache = TypePlanificateur.Aucun;
            return TypePlanificateur.Aucun;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DÉTECTION] Erreur : {ex.Message}");
            _planificateurCache = TypePlanificateur.Aucun;
            return TypePlanificateur.Aucun;
        }
    }

    /// <summary>
    /// Vérifier si systemd est disponible
    /// </summary>
    public async Task<bool> SystemdDisponibleAsync()
    {
        try
        {
            var jsonResult = await _executeur.ExecuterAsync("detecter_planificateur.py");
            
            if (string.IsNullOrEmpty(jsonResult))
                return false;

            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            if (root.TryGetProperty("systemd_disponible", out var systemdElement))
            {
                return systemdElement.GetBoolean();
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Vérifier si cron est disponible
    /// </summary>
    public async Task<bool> CronDisponibleAsync()
    {
        try
        {
            var jsonResult = await _executeur.ExecuterAsync("detecter_planificateur.py");
            
            if (string.IsNullOrEmpty(jsonResult))
                return false;

            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            if (root.TryGetProperty("cron_disponible", out var cronElement))
            {
                return cronElement.GetBoolean();
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}