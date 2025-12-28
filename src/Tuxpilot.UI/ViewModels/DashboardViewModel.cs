using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.UI.ViewModels.Extensions;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la vue Dashboard
/// </summary>
public partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly IServiceSysteme _serviceSysteme;
    private readonly IServiceAssistantIA _serviceIA; 
    private readonly IServiceHistorique _serviceHistorique; 
    private Timer? _refreshTimer;    
    private readonly Timer? _uiUpdateTimer;
    
    [ObservableProperty]
    private ObservableCollection<ActionHistorique> _dernieresActions = new();

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

    [ObservableProperty]
    private string _suggestionIA = string.Empty;
    
    [ObservableProperty]
    private bool _hasSuggestion;
    
    [ObservableProperty]
    private bool _isAnalyzing;
    public DashboardViewModel(IServiceSysteme serviceSysteme,  IServiceAssistantIA serviceIA, IServiceHistorique serviceHistorique)
    { 
        _serviceSysteme = serviceSysteme;
        _serviceIA = serviceIA;
        _serviceHistorique = serviceHistorique;
        
        _ = ChargerHistoriqueAsync(); 
        
        // Charger les données au démarrage
        _ = LoadSystemInfoAsync();
        
        _uiUpdateTimer = new Timer(
            _ => OnPropertyChanged(nameof(TimeSinceLastRefresh)),
            null,
            TimeSpan.FromSeconds(1),  // Première update après 1s
            TimeSpan.FromSeconds(1)   // Puis chaque seconde
        );
        
        _ = RefreshAsync();
        _ = AnalyserSystemeAsync(); 
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
            SystemInfo = info.ToViewModel();
            
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
    /// Analyse proactive du système
    /// </summary>
    [RelayCommand]
    private async Task AnalyserSystemeAsync()
    {
        if (SystemInfo == null) return;

        try
        {
            IsAnalyzing = true;
    
            // 🆕 Utiliser les propriétés correctes
            var ramPercent = double.Parse(SystemInfo.RamUsageText.Replace("%", "").Replace(",", "."));
            var cpuPercent = double.Parse(SystemInfo.CpuUsageText.Replace("%", "").Replace(",", "."));
            var diskPercent = double.Parse(SystemInfo.DiskUsageText.Replace("%", "").Replace(",", "."));
            
            var suggestion = await _serviceIA.AnalyserSystemeAsync(
                ramPercent,
                cpuPercent,
                diskPercent,
                0
            );
    
            SuggestionIA = suggestion;
            HasSuggestion = !suggestion.Contains("✅") && !suggestion.Contains("bien");
        }
        catch (Exception ex)
        {
            SuggestionIA = $"❌ Erreur d'analyse : {ex.Message}";
            HasSuggestion = false;
        }
        finally
        {
            IsAnalyzing = false;
        }
    }
    
    /// <summary>
    /// Charge les dernières actions de l'historique
    /// </summary>
    [RelayCommand]
    private async Task ChargerHistoriqueAsync()
    {
        try
        { 
            Console.WriteLine("[DASHBOARD] Chargement de l'historique..."); // 🆕 DEBUG

            var actions = await _serviceHistorique.ObtenirDernieresActionsAsync(5);
            Console.WriteLine($"[DASHBOARD] {actions.Count} action(s) récupérée(s)"); // 🆕 DEBUG

            DernieresActions.Clear();
            foreach (var action in actions)
            {
                DernieresActions.Add(action);
            }
            Console.WriteLine($"[DASHBOARD] DernieresActions.Count = {DernieresActions.Count}"); // 🆕 DEBUG

        }
        catch(Exception ex)
        {
            Console.WriteLine($"[DASHBOARD] ❌ ERREUR : {ex.Message}"); // 🆕 DEBUG
            Console.WriteLine($"[DASHBOARD] Stack : {ex.StackTrace}"); 
            // Ignorer les erreurs
        }
    }
    
    /// <summary>
    /// Fermer la suggestion
    /// </summary>
    [RelayCommand]
    private void FermerSuggestion()
    {
        HasSuggestion = false;
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
