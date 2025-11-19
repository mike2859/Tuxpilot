using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;

/// <summary>
/// ViewModel pour la vue Nettoyage
/// </summary>
public partial class NettoyageViewModel : ViewModelBase
{
    private readonly IServiceNettoyage _serviceNettoyage;
    
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
    
    public NettoyageViewModel(IServiceNettoyage serviceNettoyage)
    {
        _serviceNettoyage = serviceNettoyage;
        
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
            }
            
            // Notifier les propriétés calculées
            OnPropertyChanged(nameof(MessageStatut));
            OnPropertyChanged(nameof(ElementsDisponibles));
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur lors de l'analyse : {ex.Message}";
            OnPropertyChanged(nameof(MessageStatut));
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
        
        // TODO: Implémenter le nettoyage réel
        // Pour l'instant, juste un message
        await Task.Delay(2000); // Simuler le nettoyage
        
        IsSuccessVisible = true;
        await Task.Delay(3000);
        IsSuccessVisible = false;
        
        // Rafraîchir l'analyse
        await AnalyserAsync();
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