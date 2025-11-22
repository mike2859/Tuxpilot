using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Service de gestion de la planification des tâches
/// </summary>
public class ServicePlanification : IServicePlanification
{
    private readonly string _cheminFichier;
    private List<TachePlanifiee> _taches = new();

    public ServicePlanification()
    {
        // Dossier de config
        var dossierConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".tuxpilot"
        );
        
        Directory.CreateDirectory(dossierConfig);
        _cheminFichier = Path.Combine(dossierConfig, "taches.json");
        
        // Charger les tâches existantes
        _ = ChargerTachesAsync();
    }

    /// <summary>
    /// Charger les tâches depuis le fichier JSON
    /// </summary>
    private async Task ChargerTachesAsync()
    {
        try
        {
            if (File.Exists(_cheminFichier))
            {
                var json = await File.ReadAllTextAsync(_cheminFichier);
                _taches = JsonSerializer.Deserialize<List<TachePlanifiee>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANIFICATION] Erreur chargement : {ex.Message}");
            _taches = new();
        }
    }

    /// <summary>
    /// Sauvegarder les tâches dans le fichier JSON
    /// </summary>
    private async Task SauvegarderTachesAsync()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_taches, options);
            await File.WriteAllTextAsync(_cheminFichier, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANIFICATION] Erreur sauvegarde : {ex.Message}");
        }
    }

    /// <summary>
    /// Obtenir toutes les tâches planifiées
    /// </summary>
    public async Task<List<TachePlanifiee>> ObtenirTachesAsync()
    {
        await ChargerTachesAsync();
        return _taches.OrderBy(t => t.JourSemaine).ThenBy(t => t.Heure).ToList();
    }

    /// <summary>
    /// Ajouter une tâche planifiée
    /// </summary>
    public async Task<bool> AjouterTacheAsync(TachePlanifiee tache)
    {
        try
        {
            _taches.Add(tache);
            await SauvegarderTachesAsync();
            
            Console.WriteLine($"[PLANIFICATION] Tâche ajoutée : {tache.Nom}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANIFICATION] Erreur ajout : {ex.Message}");
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
            var tache = _taches.FirstOrDefault(t => t.Id == id);
            if (tache == null) return false;

            _taches.Remove(tache);
            await SauvegarderTachesAsync();
            
            Console.WriteLine($"[PLANIFICATION] Tâche supprimée : {tache.Nom}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANIFICATION] Erreur suppression : {ex.Message}");
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
            var tache = _taches.FirstOrDefault(t => t.Id == id);
            if (tache == null) return false;

            tache.Activee = !tache.Activee;
            await SauvegarderTachesAsync();
            
            Console.WriteLine($"[PLANIFICATION] Tâche {(tache.Activee ? "activée" : "désactivée")} : {tache.Nom}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANIFICATION] Erreur toggle : {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtenir une tâche par ID
    /// </summary>
    public async Task<TachePlanifiee?> ObtenirTacheAsync(string id)
    {
        await ChargerTachesAsync();
        return _taches.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>
    /// Mettre à jour une tâche
    /// </summary>
    public async Task<bool> MettreAJourTacheAsync(TachePlanifiee tache)
    {
        try
        {
            var index = _taches.FindIndex(t => t.Id == tache.Id);
            if (index == -1) return false;

            _taches[index] = tache;
            await SauvegarderTachesAsync();
            
            Console.WriteLine($"[PLANIFICATION] Tâche mise à jour : {tache.Nom}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PLANIFICATION] Erreur mise à jour : {ex.Message}");
            return false;
        }
    }
}