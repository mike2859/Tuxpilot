using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Avalonia.Media;         
using Avalonia;
using Avalonia.Controls;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la vue Mises à jour
/// </summary>
public partial class MisesAJourViewModel : ViewModelBase
{
    private readonly IServiceMisesAJour _serviceMisesAJour;
    private readonly IServiceHistorique _serviceHistorique; 
    
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
    [ObservableProperty]
    private bool _isInstallingWithLogs;

    [ObservableProperty]
    private bool _isLogsExpanded = true;

    [ObservableProperty]
    private ObservableCollection<InstallLogViewModel> _logs = new();

    [ObservableProperty]
    private int _packagesInstalled;

    [ObservableProperty]
    private int _totalPackages;

    [ObservableProperty]
    private double _progressPercent;
    
    
    
    public ICommand ToggleLogsCommand => new RelayCommand(() => IsLogsExpanded = !IsLogsExpanded);
    
    public MisesAJourViewModel(IServiceMisesAJour serviceMisesAJour, IServiceHistorique  serviceHistorique)
    {
        _serviceMisesAJour = serviceMisesAJour;
        _serviceHistorique = serviceHistorique; 
        
        // Charger les mises à jour au démarrage
        _ = VerifierMisesAJourAsync();
    }
    
    /// <summary>
    /// Nom de la ressource d'icône selon le statut
    /// </summary>
    public string MessageStatutIconeResourceKey
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return "IconClose";
            
            if (NombreMisesAJour == 0)
                return "IconCheck";
            
            return "IconWarning";
        }
    }
    
    /// <summary>
    /// Couleur de l'icône selon le statut
    /// </summary>
    public string MessageStatutIconeColor
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return "Danger";
            
            if (NombreMisesAJour == 0)
                return "Success";
            
            return "Warning";
        }
    }
    
    /// <summary>
    /// Message de statut (texte seulement)
    /// </summary>
    public string MessageStatutTexte
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return MessageErreur;
            
            if (NombreMisesAJour == 0)
                return "Votre système est à jour !";
            
            if (NombreMisesAJour == 1)
                return "1 mise à jour disponible";
            
            return $"{NombreMisesAJour} mises à jour disponibles";
        }
    }
    
    /// <summary>
    /// Indique si des mises à jour sont disponibles
    /// </summary>
    public bool MisesAJourDisponibles => NombreMisesAJour > 0;
    
    
    /// <summary>
    /// Couleur de fond du message de statut
    /// </summary>
    public string BackgroundColor => MisesAJourDisponibles ? "BackgroundWarning" : "BackgroundSuccess";

    /// <summary>
    /// Couleur de bordure du message de statut
    /// </summary>
    public string BorderColor => MisesAJourDisponibles ? "Warning" : "Success";
    
// ✅ AJOUTER CES 2 PROPRIÉTÉS ICI
    /// <summary>
    /// Type de message de statut (pour déterminer les couleurs)
    /// </summary>
    public string MessageStatutType
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return "error";
        
            if (NombreMisesAJour == 0)
                return "success";
        
            return "warning";
        }
    }

    /// <summary>
    /// Ressource de couleur du texte selon le type de message
    /// </summary>
    public IBrush? MessageStatutTextBrush
    {
        get
        {
            var resourceKey = MessageStatutType switch
            {
                "success" => "SuccessTextOnBackground",
                "warning" => "WarningTextOnBackground",
                "error" => "ErrorTextOnBackground",
                "info" => "InfoTextOnBackground",
                _ => "TextPrimary"
            };

            if (Application.Current?.TryFindResource(resourceKey, out var resource) == true)
            {
                return resource as IBrush;
            }

            return Brushes.Black;
        }
    }
    
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
            TotalPackages = updateInfo.Nombre; 
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
            OnPropertyChanged(nameof(MessageStatutIconeResourceKey));
            OnPropertyChanged(nameof(MessageStatutIconeColor));
            OnPropertyChanged(nameof(MessageStatutTexte));
            OnPropertyChanged(nameof(MessageStatutType));          
            OnPropertyChanged(nameof(MessageStatutTextBrush));     
            OnPropertyChanged(nameof(MisesAJourDisponibles));
            OnPropertyChanged(nameof(BackgroundColor));     
            OnPropertyChanged(nameof(BorderColor));
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur lors de la vérification : {ex.Message}";
            OnPropertyChanged(nameof(MessageStatutIconeResourceKey));
            OnPropertyChanged(nameof(MessageStatutIconeColor));
            OnPropertyChanged(nameof(MessageStatutTexte));
            OnPropertyChanged(nameof(MessageStatutType));           
            OnPropertyChanged(nameof(MessageStatutTextBrush));  
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
    // Masquer la confirmation
    IsConfirmationVisible = false;
    
    // Afficher la vue avec logs
    IsInstallingWithLogs = true;
    Logs.Clear();
    PackagesInstalled = 0;
    ProgressPercent = 0;
    
    try
    {
        // Installer avec callback pour les logs
        var (success, message) = await _serviceMisesAJour.InstallerMisesAJourAsync(
            onLogReceived: (type, msg) =>
            {
                // Ajouter le log à la collection (sur le thread UI)
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    Logs.Add(new InstallLogViewModel(type, msg));
                    
                    // Mettre à jour la progression
                    if (type == "install" || type == "success")
                    {
                        PackagesInstalled++;
                        ProgressPercent = TotalPackages > 0 
                            ? (double)PackagesInstalled / TotalPackages * 100 
                            : 0;
                    }
                });
            }
        );
        
        // Attendre 2 secondes pour voir le dernier message
        await Task.Delay(2000);
        
        await _serviceHistorique.AjouterActionAsync(
            TypeAction.Update,
            $"{PackagesInstalled} mise(s) à jour installée(s)",
            true
        );
        // Masquer les logs
        IsInstallingWithLogs = false;
        
        if (success)
        {
            MessageErreur = null;
            
            // Afficher le message de succès
            IsSuccessVisible = true;
            await Task.Delay(3000);
            IsSuccessVisible = false;

            // Rafraîchir la liste
            await VerifierMisesAJourAsync();
        }
        else
        {
            // En cas d'erreur
            MessageErreur = message;
            OnPropertyChanged(nameof(MessageStatutIconeResourceKey));
            OnPropertyChanged(nameof(MessageStatutIconeColor));
            OnPropertyChanged(nameof(MessageStatutTexte));
        }
    }
    catch (Exception ex)
    {
        MessageErreur = $"Erreur : {ex.Message}";
        OnPropertyChanged(nameof(MessageStatutIconeResourceKey));
        OnPropertyChanged(nameof(MessageStatutIconeColor));
        OnPropertyChanged(nameof(MessageStatutTexte));
        OnPropertyChanged(nameof(MessageStatutType));           
        OnPropertyChanged(nameof(MessageStatutTextBrush));     
        IsInstallingWithLogs = false;
        
        await _serviceHistorique.AjouterActionAsync(
            TypeAction.Update,
            "Échec de l'installation des mises à jour",
            false
        );
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