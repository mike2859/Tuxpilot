using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la vue Dashboard
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private SystemInfoViewModel _systemInfo = new();
    
    [ObservableProperty]
    private string? _statusMessage;
    
    public DashboardViewModel()
    {
        // Charger les données au démarrage
        _ = LoadSystemInfoAsync();
    }
    
    /// <summary>
    /// Charge les informations système
    /// </summary>
    private async Task LoadSystemInfoAsync()
    {
        try
        {
            StatusMessage = "Chargement des informations système...";
            
            // Simuler un chargement (à remplacer par le vrai service)
            await Task.Delay(500);
            
            // TODO: Appeler le vrai service
            // var info = await _systemService.GetSystemInfoAsync();
            
            // Données de test pour l'instant
            SystemInfo = new SystemInfoViewModel
            {
                Distribution = "Fedora 41 (Forty One)",
                KernelVersion = "6.17.7-100.fc41.x86_64",
                TotalRamMB = 62000,
                UsedRamMB = 7000,
                RamPercent = 11.3,
                TotalDiskGB = 500,
                UsedDiskGB = 150,
                DiskPercent = 30
            };
            
            StatusMessage = "Système opérationnel ✅";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }
    
    /// <summary>
    /// Commande pour rafraîchir les données
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadSystemInfoAsync();
    }
}
