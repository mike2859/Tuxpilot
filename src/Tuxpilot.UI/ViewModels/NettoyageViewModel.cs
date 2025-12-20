using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Avalonia.Media;    
using Avalonia;
using Avalonia.Controls;

namespace Tuxpilot.UI.ViewModels;

/// <summary>
/// ViewModel pour la vue Nettoyage
/// </summary>
public partial class NettoyageViewModel : ViewModelBase
{
    private readonly IServiceNettoyage _serviceNettoyage;
    private readonly IServiceHistorique _serviceHistorique;
    
    [ObservableProperty]
    private ObservableCollection<CleanupElementViewModel> _elements = new();
    
    [ObservableProperty]
    private long _tailleTotaleMB;
    
    [ObservableProperty]
    private string _gestionnaire = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _messageErreur;
    
    [ObservableProperty]
    private bool _isConfirmationVisible;
    
    [ObservableProperty]
    private bool _isSuccessVisible;
    
    public NettoyageViewModel(IServiceNettoyage serviceNettoyage, IServiceHistorique serviceHistorique)
    {
        _serviceNettoyage = serviceNettoyage;
        _serviceHistorique = serviceHistorique;
        
        // Charger les données au démarrage
        _ = AnalyserAsync();
    }
    
    /// <summary>
    /// Message de statut selon les éléments
    /// </summary>
    public string MessageStatut
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return $"❌ {MessageErreur}";
            
            if (TailleTotaleMB == 0)
                return "✅ Aucun nettoyage nécessaire";
            
            var tailleGB = TailleTotaleMB / 1024.0;
            if (tailleGB < 1)
                return $"🧹 {TailleTotaleMB} MB peuvent être libérés";
            else
                return $"🧹 {tailleGB:F1} GB peuvent être libérés";
        }
    }
    
    /// <summary>
    /// Couleur de fond du message
    /// </summary>
    public string BackgroundColor => TailleTotaleMB > 0 ? "#FEF3C7" : "#ECFDF5";
    
    /// <summary>
    /// Couleur de bordure du message
    /// </summary>
    public string BorderColor => TailleTotaleMB > 0 ? "#F59E0B" : "#10B981";
    
    // ✅ AJOUTER CES 2 PROPRIÉTÉS ICI
    /// <summary>
    /// Type de message de statut
    /// </summary>
    public string MessageStatutType
    {
        get
        {
            if (!string.IsNullOrEmpty(MessageErreur))
                return "error";
        
            if (TailleTotaleMB == 0)
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
    /// Indique si des éléments sont disponibles
    /// </summary>
    public bool ElementsDisponibles => Elements.Count > 0;
    
    /// <summary>
    /// Analyse les éléments nettoyables
    /// </summary>
    [RelayCommand]
    private async Task AnalyserAsync()
    {
        try
        {
            IsLoading = true;
            MessageErreur = null;
            
            var cleanupInfo = await _serviceNettoyage.AnalyserNettoyageAsync();
            
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Clean,
                $"Commande AnalyserNettoyageAsync exécutée : {cleanupInfo.NombreElements} élément(s)",
                true
            );
            
            // Mettre à jour les propriétés
            Gestionnaire = cleanupInfo.Gestionnaire;
            TailleTotaleMB = cleanupInfo.TailleTotaleMB;
            MessageErreur = cleanupInfo.Erreur;
            
            // Mapper les éléments
            Elements.Clear();
            foreach (var element in cleanupInfo.Elements)
            {
                Elements.Add(new CleanupElementViewModel
                {
                    Type = element.Type,
                    Nom = element.Nom,
                    Chemin = element.Chemin,
                    TailleMB = element.TailleMB,
                    NombreFichiers = element.NombreFichiers,
                    NombrePaquets = element.NombrePaquets,
                    Description = element.Description
                });
                
                await _serviceHistorique.AjouterActionAsync(
                    TypeAction.Clean,
                    $"Element: {element.Nom} {element.Type}",
                    true
                );
            }
        
            // Notifier les propriétés calculées
            OnPropertyChanged(nameof(MessageStatut));
            OnPropertyChanged(nameof(MessageStatutType));        
            OnPropertyChanged(nameof(MessageStatutTextBrush));  
            OnPropertyChanged(nameof(ElementsDisponibles));
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur lors de l'analyse : {ex.Message}";
            OnPropertyChanged(nameof(MessageStatut));
            OnPropertyChanged(nameof(MessageStatutType));       
            OnPropertyChanged(nameof(MessageStatutTextBrush));  
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Clean,
                $"Échec AnalyserNettoyageAsync : {MessageErreur}",
                false
            );
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Commande pour nettoyer
    /// </summary>
    [RelayCommand]
    private void Nettoyer()
    {
        if (TailleTotaleMB == 0)
            return;
        
        // Afficher le dialogue de confirmation
        IsConfirmationVisible = true;
    }
    
    /// <summary>
    /// Commande pour confirmer le nettoyage
    /// </summary>
    [RelayCommand]
    private async Task ConfirmerNettoyageAsync()
    {
        IsConfirmationVisible = false;
        IsLoading = true;
    
        try
        {
            // Nettoyer réellement le système
            var (success, message) = await _serviceNettoyage.NettoyerAsync();
        
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Clean,
                message,
                success
            );
        
            if (success)
            {
                // Afficher le message de succès
                IsSuccessVisible = true;
                await Task.Delay(3000);
                IsSuccessVisible = false;
            
                // Rafraîchir l'analyse pour voir la différence
                await AnalyserAsync();
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
        
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Clean,
                $"Échec du nettoyage: {MessageErreur}",
                false
            );
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Commande pour annuler le nettoyage
    /// </summary>
    [RelayCommand]
    private void AnnulerNettoyage()
    {
        IsConfirmationVisible = false;
    }
}