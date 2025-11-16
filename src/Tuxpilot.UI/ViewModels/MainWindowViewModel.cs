using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceSysteme _serviceSysteme;
    
    [ObservableProperty]
    private SystemInfo? _infoSysteme;
    
    [ObservableProperty]
    private string _messageStatut = "Chargement...";
    
    [ObservableProperty]
    private bool _chargementEnCours = true;
    
    public MainWindowViewModel(IServiceSysteme serviceSysteme)
    {
        _serviceSysteme = serviceSysteme;
        
        // Charger les infos au démarrage
        _ = ChargerInfoSystemeAsync();
    }
    
    private async Task ChargerInfoSystemeAsync()
    {
        try
        {
            ChargementEnCours = true;
            MessageStatut = "Détection du système en cours...";
            
            InfoSysteme = await _serviceSysteme.ObtenirInfoSystemeAsync();
            
            var statut = InfoSysteme.ObtenirStatut();
            MessageStatut = statut switch
            {
                StatutSysteme.Sain => "✅ Système en bonne santé",
                StatutSysteme.Avertissement => "⚠️ Attention requise",
                StatutSysteme.Critique => "🔴 Action urgente nécessaire",
                _ => "Système détecté"
            };
            
            ChargementEnCours = false;
        }
        catch (Exception ex)
        {
            MessageStatut = $"❌ Erreur : {ex.Message}";
            ChargementEnCours = false;
        }
    }
}
