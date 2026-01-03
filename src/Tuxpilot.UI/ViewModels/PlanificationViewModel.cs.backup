using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Services;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la planification des tâches
/// </summary>
public partial class PlanificationViewModel : ViewModelBase
{
    private readonly ServicePlanificationFactory _factory;
    private IServicePlanification? _servicePlanification;


    [ObservableProperty]
    private ObservableCollection<TachePlanifiee> _taches = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Propriétés pour la nouvelle tâche
    [ObservableProperty]
    private TypeTache _typeTacheSelectionnee = TypeTache.MisesAJour;

    [ObservableProperty]
    private int _jourSelectionne = 2; // Lundi par défaut

    [ObservableProperty]
    private int _heureSelectionnee = 2; // 02h par défaut

    [ObservableProperty]
    private int _minuteSelectionnee = 0;

    [ObservableProperty]
    private bool _showAddDialog;

    public PlanificationViewModel(ServicePlanificationFactory factory)
    {
        _factory = factory;
        _ = ChargerTachesAsync();
        
    }

    private async Task<IServicePlanification> ObtenirServiceAsync()
    {
        if (_servicePlanification == null)
        {
            _servicePlanification = await _factory.ObtenirServiceAsync();
        }
        return _servicePlanification;
    }
    
    /// <summary>
    /// Charger les tâches planifiées
    /// </summary>
    [RelayCommand]
    private async Task ChargerTachesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Chargement des tâches...";

            var service = await ObtenirServiceAsync();
            var taches = await service.ObtenirTachesAsync();
            
            Taches.Clear();
            foreach (var tache in taches)
            {
                Taches.Add(tache);
            }

            StatusMessage = $"{Taches.Count} tâche(s) planifiée(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Afficher le dialogue d'ajout
    /// </summary>
    [RelayCommand]
    private void AfficherAjout()
    {
        ShowAddDialog = true;
    }

    /// <summary>
    /// Annuler l'ajout
    /// </summary>
    [RelayCommand]
    private void AnnulerAjout()
    {
        ShowAddDialog = false;
    }

    /// <summary>
    /// Ajouter une nouvelle tâche
    /// </summary>
    [RelayCommand]
    private async Task AjouterTacheAsync()
    {
        try
        {
            var nomTache = TypeTacheSelectionnee switch
            {
                TypeTache.MisesAJour => "Mises à jour automatiques",
                TypeTache.Nettoyage => "Nettoyage système",
                TypeTache.Rapport => "Rapport système",
                _ => "Tâche planifiée"
            };

            var description = TypeTacheSelectionnee switch
            {
                TypeTache.MisesAJour => "Vérifier et installer les mises à jour système",
                TypeTache.Nettoyage => "Nettoyer le cache et les fichiers temporaires",
                TypeTache.Rapport => "Générer un rapport de l'état du système",
                _ => ""
            };

            var nouvelleTache = new TachePlanifiee
            {
                Type = TypeTacheSelectionnee,
                Nom = nomTache,
                Description = description,
                JourSemaine = JourSelectionne - 1,  
                Heure = HeureSelectionnee,
                Minute = MinuteSelectionnee,
                Activee = true
            };

            var success = await _servicePlanification.AjouterTacheAsync(nouvelleTache);
            
            if (success)
            {
                Taches.Add(nouvelleTache);
                StatusMessage = $"✅ Tâche ajoutée : {nomTache}";
                ShowAddDialog = false;
            }
            else
            {
                StatusMessage = "❌ Erreur lors de l'ajout de la tâche";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Supprimer une tâche
    /// </summary>
    [RelayCommand]
    private async Task SupprimerTacheAsync(string id)
    {
        try
        {
            var success = await _servicePlanification.SupprimerTacheAsync(id);
            
            if (success)
            {
                var tache = Taches.FirstOrDefault(t => t.Id == id);
                if (tache != null)
                {
                    Taches.Remove(tache);
                    StatusMessage = $"✅ Tâche supprimée";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Activer/Désactiver une tâche
    /// </summary>
    [RelayCommand]
    private async Task ToggleTacheAsync(string id)
    {
        try
        {
            var success = await _servicePlanification.ToggleTacheAsync(id);
            
            if (success)
            {
                var tache = Taches.FirstOrDefault(t => t.Id == id);
                if (tache != null)
                {
                    tache.Activee = !tache.Activee;
                    await ChargerTachesAsync(); // Recharger pour rafraîchir l'affichage
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur : {ex.Message}";
        }
    }

    /// <summary>
    /// Liste des jours de la semaine
    /// </summary>
    public string[] JoursSemaine => new[]
    {
        "Tous les jours", "Dimanche", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi"
    };

    /// <summary>
    /// Liste des types de tâches
    /// </summary>
    public TypeTache[] TypesTaches => Enum.GetValues<TypeTache>();
}