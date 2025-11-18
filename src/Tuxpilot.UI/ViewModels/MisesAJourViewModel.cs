using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la vue Mises à jour
/// </summary>
public partial class MisesAJourViewModel : ViewModelBase
{
    private readonly IServiceMisesAJour _serviceMisesAJour;
    
    [ObservableProperty]
    private ObservableCollection<PackageViewModel> _paquets = new();
    
    [ObservableProperty]
    private int _nombreMisesAJour;
    
    [ObservableProperty]
    private string _gestionnaire = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _messageErreur;
    
    [ObservableProperty]
    private DateTime _derniereMiseAJour = DateTime.Now;
    
    [ObservableProperty]
    private bool _isConfirmationVisible;

    [ObservableProperty]
    private bool _isSuccessVisible;
    
    public MisesAJourViewModel(IServiceMisesAJour serviceMisesAJour)
    {
        _serviceMisesAJour = serviceMisesAJour;
        
        // Charger les mises à jour au démarrage
        _ = VerifierMisesAJourAsync();
    }
    
    /// <summary>
    /// Message de statut selon le nombre de mises à jour
    /// </summary>
    public string MessageStatut
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return $"❌ {MessageErreur}";
            
            if (NombreMisesAJour == 0)
                return "✅ Votre système est à jour !";
            
            if (NombreMisesAJour == 1)
                return "⚠️ 1 mise à jour disponible";
            
            return $"⚠️ {NombreMisesAJour} mises à jour disponibles";
        }
    }
    /// <summary>
    /// Indique si des mises à jour sont disponibles
    /// </summary>
    public bool MisesAJourDisponibles => NombreMisesAJour > 0;
    
    
    /// <summary>
    /// Couleur de fond du message de statut
    /// </summary>
    public string BackgroundColor => MisesAJourDisponibles ? "#FEF3C7" : "#ECFDF5";

    /// <summary>
    /// Couleur de bordure du message de statut
    /// </summary>
    public string BorderColor => MisesAJourDisponibles ? "#F59E0B" : "#10B981";
    
    
    /// <summary>
    /// Vérifie les mises à jour disponibles
    /// </summary>
    [RelayCommand]
    private async Task VerifierMisesAJourAsync()
    {
        try
        {
            IsLoading = true;
            MessageErreur = null;
            
            var updateInfo = await _serviceMisesAJour.VerifierMisesAJourAsync();
            
            // Mettre à jour les propriétés
            Gestionnaire = updateInfo.Gestionnaire;
            NombreMisesAJour = updateInfo.Nombre;
            MessageErreur = updateInfo.Erreur;
            
            // Mapper les paquets
            Paquets.Clear();
            foreach (var paquet in updateInfo.Paquets)
            {
                Paquets.Add(new PackageViewModel
                {
                    Nom = paquet.Nom,
                    VersionActuelle = paquet.VersionActuelle,
                    VersionDisponible = paquet.VersionDisponible,
                    Depot = paquet.Depot
                });
            }
            
            DerniereMiseAJour = DateTime.Now;
            
            // Notifier les propriétés calculées
            OnPropertyChanged(nameof(MessageStatut));
            OnPropertyChanged(nameof(MisesAJourDisponibles));
            OnPropertyChanged(nameof(BackgroundColor));     
            OnPropertyChanged(nameof(BorderColor));   
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur lors de la vérification : {ex.Message}";
            OnPropertyChanged(nameof(MessageStatut));
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Commande pour installer toutes les mises à jour
    /// </summary>
    [RelayCommand]
    private async Task InstallerToutAsync()
    {
        // Impossible si aucune mise à jour
        if (NombreMisesAJour == 0)
            return;
    
        // Afficher le dialogue de confirmation
        IsConfirmationVisible = true;
    }

    /// <summary>
    /// Commande pour confirmer l'installation
    /// </summary>
    [RelayCommand]
    private async Task ConfirmerInstallationAsync()
    {
        IsConfirmationVisible = false;
    
        try
        {
            IsLoading = true;
            MessageErreur = "Installation en cours... Veuillez patienter.";
            OnPropertyChanged(nameof(MessageStatut));
        
            var (success, message) = await _serviceMisesAJour.InstallerMisesAJourAsync();
        
            if (success)
            {
                MessageErreur = null;
            
                // Rafraîchir la liste des mises à jour
                await VerifierMisesAJourAsync();
            
                // Afficher le succès
                IsSuccessVisible = true;
                await Task.Delay(3000); // Afficher 3 secondes
                IsSuccessVisible = false;
            }
            else
            {
                MessageErreur = message;
                OnPropertyChanged(nameof(MessageStatut));
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
            OnPropertyChanged(nameof(MessageStatut));
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Commande pour annuler l'installation
    /// </summary>
    [RelayCommand]
    private void AnnulerInstallation()
    {
        IsConfirmationVisible = false;
    }
}