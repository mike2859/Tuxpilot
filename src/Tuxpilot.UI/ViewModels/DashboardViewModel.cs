using System;
using System.Threading;
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
    private Timer? _refreshTimer;    
    private readonly Timer? _uiUpdateTimer;
    
    [ObservableProperty]
    private SystemInfoViewModel _systemInfo = new();
    
    [ObservableProperty]
    private string? _statusMessage;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private DateTime _lastRefreshTime = DateTime.Now;
    
    [ObservableProperty]
    private bool _autoRefreshEnabled = true;

    public DashboardViewModel(IServiceSysteme serviceSysteme)
    { 
        _serviceSysteme = serviceSysteme;

        // Charger les données au démarrage
        _ = LoadSystemInfoAsync();
        
        _uiUpdateTimer = new Timer(
            _ => OnPropertyChanged(nameof(TimeSinceLastRefresh)),
            null,
            TimeSpan.FromSeconds(1),  // Première update après 1s
            TimeSpan.FromSeconds(1)   // Puis chaque seconde
        );
    }
    
    
    /// <summary>
    /// Texte affichant le temps depuis la dernière mise à jour
    /// </summary>
    public string TimeSinceLastRefresh
    {
        get
        {
            var elapsed = DateTime.Now - LastRefreshTime;
            
            if (elapsed.TotalSeconds < 60)
                return $"Mis à jour il y a {elapsed.Seconds}s";
            else if (elapsed.TotalMinutes < 60)
                return $"Mis à jour il y a {elapsed.Minutes}min";
            else
                return $"Mis à jour il y a {elapsed.Hours}h";
        }
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
                CpuModel = info.CpuModel,      
                CpuCores = info.CpuCores,     
                CpuThreads = info.CpuThreads,  
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
            
            // Mettre à jour l'heure
            LastRefreshTime = DateTime.Now;
            OnPropertyChanged(nameof(TimeSinceLastRefresh));
            
            if (_refreshTimer == null)
            {
                _refreshTimer = new Timer(
                    async _ => await LoadSystemInfoAsync(),
                    null,
                    TimeSpan.FromSeconds(10),  // Dans 10s exactement
                    TimeSpan.FromSeconds(10)   // Puis toutes les 10s
                );
            }
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
    
    /// <summary>
    /// Commande pour activer/désactiver l'auto-refresh
    /// </summary>
    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        AutoRefreshEnabled = !AutoRefreshEnabled;
    }
    
    // Nettoyer le timer quand le ViewModel est détruit
    public void Dispose()
    {
        _refreshTimer?.Dispose();
        _uiUpdateTimer?.Dispose(); 
    }
}
