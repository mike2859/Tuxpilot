using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Service de planification utilisant cron
/// </summary>
public class ServicePlanificationCron : IServicePlanification
{
    private readonly ExecuteurScriptPython _executeur;
    private const string SCRIPT_NAME = "planification_cron.py";

    public ServicePlanificationCron(ExecuteurScriptPython executeur)
    {
        _executeur = executeur;
    }

    /// <summary>
    /// Obtenir toutes les tâches planifiées
    /// </summary>
    public async Task<List<TachePlanifiee>> ObtenirTachesAsync()
    {
        try
        {
            var jsonResult = await _executeur.ExecuterAsync(SCRIPT_NAME, "lister");
            
            if (string.IsNullOrEmpty(jsonResult))
            {
                Console.WriteLine("[CRON] Aucun résultat");
                return new List<TachePlanifiee>();
            }

            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            if (!root.TryGetProperty("taches", out var tachesElement))
            {
                return new List<TachePlanifiee>();
            }

            var taches = new List<TachePlanifiee>();
            foreach (var tacheJson in tachesElement.EnumerateArray())
            {
                var tache = ParseTache(tacheJson);
                if (tache != null)
                {
                    taches.Add(tache);
                }
            }

            return taches.OrderBy(t => t.JourSemaine).ThenBy(t => t.Heure).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRON] Erreur obtention tâches : {ex.Message}");
            return new List<TachePlanifiee>();
        }
    }

    /// <summary>
    /// Ajouter une tâche planifiée
    /// </summary>
    public async Task<bool> AjouterTacheAsync(TachePlanifiee tache)
    {
        try
        {
            // Convertir en JSON
            var tacheJson = JsonSerializer.Serialize(new
            {
                id = tache.Id,
                type = tache.Type.ToString(),
                nom = tache.Nom,
                description = tache.Description,
                jour_semaine = tache.JourSemaine,
                heure = tache.Heure,
                minute = tache.Minute,
                activee = tache.Activee
            });

            // Créer un fichier temporaire
            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"tuxpilot_task_{Guid.NewGuid()}.json");
            await System.IO.File.WriteAllTextAsync(tempFile, tacheJson);

            try
            {
                var jsonResult = await _executeur.ExecuterAsync(SCRIPT_NAME, $"ajouter {tempFile}");
                
                if (string.IsNullOrEmpty(jsonResult))
                {
                    Console.WriteLine("[CRON] Résultat vide");
                    return false;
                }

                var json = JsonDocument.Parse(jsonResult);
                var root = json.RootElement;

                if (root.TryGetProperty("success", out var success))
                {
                    var result = success.GetBoolean();
                    Console.WriteLine($"[CRON] Résultat ajout: {result}");
                    return result;
                }

                return false;
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFile))
                    {
                        System.IO.File.Delete(tempFile);
                    }
                }
                catch { /* Ignore */ }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRON] Erreur ajout : {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Supprimer une tâche planifiée
    /// </summary>
    public async Task<bool> SupprimerTacheAsync(string id)
    {
        try
        {
            var jsonResult = await _executeur.ExecuterAsync(SCRIPT_NAME, $"supprimer {id}");
            
            if (string.IsNullOrEmpty(jsonResult))
            {
                return false;
            }

            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            if (root.TryGetProperty("success", out var success))
            {
                return success.GetBoolean();
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRON] Erreur suppression : {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Activer/Désactiver une tâche
    /// </summary>
    public async Task<bool> ToggleTacheAsync(string id)
    {
        try
        {
            var jsonResult = await _executeur.ExecuterAsync(SCRIPT_NAME, $"toggle {id}");
            
            if (string.IsNullOrEmpty(jsonResult))
            {
                return false;
            }

            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            if (root.TryGetProperty("success", out var success))
            {
                return success.GetBoolean();
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRON] Erreur toggle : {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtenir une tâche par ID
    /// </summary>
    public async Task<TachePlanifiee?> ObtenirTacheAsync(string id)
    {
        var taches = await ObtenirTachesAsync();
        return taches.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>
    /// Mettre à jour une tâche
    /// </summary>
    public async Task<bool> MettreAJourTacheAsync(TachePlanifiee tache)
    {
        await SupprimerTacheAsync(tache.Id);
        return await AjouterTacheAsync(tache);
    }

    /// <summary>
    /// Parser une tâche depuis JSON
    /// </summary>
    private TachePlanifiee? ParseTache(JsonElement json)
    {
        try
        {
            var tache = new TachePlanifiee();

            if (json.TryGetProperty("id", out var id))
                tache.Id = id.GetString() ?? Guid.NewGuid().ToString();

            if (json.TryGetProperty("type", out var type))
            {
                var typeStr = type.GetString() ?? "MisesAJour";
                tache.Type = Enum.TryParse<TypeTache>(typeStr, out var t) ? t : TypeTache.MisesAJour;
            }

            if (json.TryGetProperty("nom", out var nom))
                tache.Nom = nom.GetString() ?? "";

            if (json.TryGetProperty("description", out var desc))
                tache.Description = desc.GetString() ?? "";

            if (json.TryGetProperty("jour_semaine", out var jour))
                tache.JourSemaine = jour.GetInt32();

            if (json.TryGetProperty("heure", out var heure))
                tache.Heure = heure.GetInt32();

            if (json.TryGetProperty("minute", out var minute))
                tache.Minute = minute.GetInt32();

            if (json.TryGetProperty("activee", out var activee))
                tache.Activee = activee.GetBoolean();

            if (json.TryGetProperty("date_creation", out var dateStr))
            {
                if (DateTime.TryParse(dateStr.GetString(), out var date))
                    tache.DateCreation = date;
            }

            return tache;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRON] Erreur parsing tâche : {ex.Message}");
            return null;
        }
    }
}