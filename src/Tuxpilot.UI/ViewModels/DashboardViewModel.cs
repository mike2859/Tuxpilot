using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la vue Dashboard
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IServiceSysteme _serviceSysteme;

    [ObservableProperty]
    private SystemInfoViewModel _systemInfo = new();
    
    [ObservableProperty]
    private string? _statusMessage;
    
    [ObservableProperty]
    private bool _isLoading;
    public DashboardViewModel(IServiceSysteme serviceSysteme)
    { 
        _serviceSysteme = serviceSysteme;

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
            IsLoading = true;
            StatusMessage = "Chargement des informations système...";
          
            var info = await _serviceSysteme.ObtenirInfoSystemeAsync();

            // Mapper vers le ViewModel
            SystemInfo = new SystemInfoViewModel
            {
                Distribution = info.Distribution,
                KernelVersion = info.VersionKernel,
                TotalRamMB = info.RamTotaleMB,
                UsedRamMB = info.RamUtiliseeMB,
                RamPercent = info.PourcentageRam,
                CpuPercent = info.PourcentageCpu,
                DiskPercent = info.PourcentageDisque,
                PackageManager = info.GestionnairePaquets
            };
            
            var statut = info.ObtenirStatut();
            StatusMessage = statut switch
            {
                Core.Enums.StatutSysteme.Sain => "✅ Système en bon état",
                Core.Enums.StatutSysteme.Avertissement => "⚠️ Attention : ressources élevées",
                Core.Enums.StatutSysteme.Critique => "🔴 Critique : action nécessaire",
                _ => "✅ Système opérationnel"
            };
            
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur : {ex.Message}";
            SystemInfo = new SystemInfoViewModel
            {
                Distribution = "Erreur de chargement",
                KernelVersion = ex.Message
            };
        }
        finally
        {
            IsLoading = false;
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
