using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.UI.Views;

namespace Tuxpilot.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceTheme _serviceTheme;
    private readonly ILicenseService _licenseService;
 
    [ObservableProperty]
    private bool _isProFeatureEnabled;

    [ObservableProperty]
    private string _proFeatureTooltip = "🔒 Fonctionnalité Pro - Activez une licence Pro pour débloquer";
    
    [ObservableProperty]
    private object? _currentView;
    
    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";

    [ObservableProperty]
    private bool _estThemeSombre;
    
    public MainWindowViewModel(IServiceProvider serviceProvider, IServiceTheme serviceTheme, ILicenseService licenseService)
    {
        _serviceProvider = serviceProvider;
        _serviceTheme = serviceTheme;
        _licenseService = licenseService; 
        
        EstThemeSombre = _serviceTheme.ThemeActuel == Theme.Dark;
        
        // Charger les features
        _ = LoadFeaturesAsync();
        
        CurrentView = ObtenirDashboardViewModel();
    }
    
    /// <summary>
    /// Charge l'état des features Pro/Community
    /// </summary>
    private async Task LoadFeaturesAsync()
    {
        // Vérifier si Pro
        IsProFeatureEnabled = await _licenseService.HasFeatureAsync("ai_assistant_unlimited");
    
        // Message selon le statut
        if (!IsProFeatureEnabled)
        {
            ProFeatureTooltip = "🔒 Fonctionnalité Pro - Activez une licence Pro pour débloquer";
        }
        else
        {
            ProFeatureTooltip = "Fonctionnalité Pro activée ✅";
        }
    }
    
    /// <summary>
    /// Recharge les features après activation de licence
    /// </summary>
    public async Task RefreshLicenseFeaturesAsync()
    {
        await LoadFeaturesAsync();
    }

    private DashboardViewModel ObtenirDashboardViewModel()
    {
        var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
        return dashboardViewModel;
    }
    
    /// <summary>
    /// Commande pour changer le thème
    /// </summary>
    [RelayCommand]
    private void ChangerTheme()
    {
        var nouveauTheme = EstThemeSombre ? Theme.Light : Theme.Dark;
    
        _serviceTheme.ChangerTheme(nouveauTheme);
        EstThemeSombre = nouveauTheme == Theme.Dark;
    
        // Appliquer le thème
        if (App.Current is App app)
        {
            app.AppliquerTheme(nouveauTheme);
        }
    
        // Notifier le changement d'icône
        OnPropertyChanged(nameof(EstThemeSombre));
    }
    
    /// <summary>
    /// Commande pour naviguer vers le Dashboard
    /// </summary>
    [RelayCommand]
    private void NavigateToDashboard()
        => CurrentView = ObtenirDashboardViewModel();
    
    
    /// <summary>
    /// Commande pour naviguer vers les Mises à jour
    /// </summary>
    [RelayCommand]
    private void NavigateToUpdates()
    {
        var misesAJourViewModel = _serviceProvider.GetRequiredService<MisesAJourViewModel>();
        var misesAJourView = new MisesAJourView
        {
            DataContext = misesAJourViewModel
        };
    
        CurrentView = misesAJourView;
    }
    
    /// <summary>
    /// Commande pour naviguer vers le Nettoyage
    /// </summary>
    [RelayCommand]
    private void NavigateToCleanup()
    {
        var nettoyageViewModel = _serviceProvider.GetRequiredService<NettoyageViewModel>();
        var nettoyageView = new NettoyageView
        {
            DataContext = nettoyageViewModel
        };
    
        CurrentView = nettoyageView;
    }
    
    /// <summary>
    /// Commande pour naviguer vers le Diagnostic
    /// </summary>
    [RelayCommand]
    private void NavigateToDiagnostic()
    {
        // Créer le ViewModel via DI
        var diagnosticViewModel = _serviceProvider.GetRequiredService<DiagnosticViewModel>();
        var diagnosticView = new DiagnosticView
        {
            DataContext = diagnosticViewModel
        };
    
        CurrentView = diagnosticView;
    }
    
    /// <summary>
    /// Commande pour naviguer vers l'Assistant IA (Pro uniquement)
    /// </summary>
    [RelayCommand]
    private void NavigateToAssistant()
    {
        // ✅ CORRIGÉ : Ne pas revérifier à chaque fois, on utilise IsProFeatureEnabled
        // qui est déjà chargé au démarrage et après activation
        if (IsProFeatureEnabled)
        {
            CurrentView = _serviceProvider.GetRequiredService<AssistantIAViewModel>();
        }
        // Sinon, le bouton est désactivé dans le XAML donc on ne devrait jamais arriver ici
    }
    
    /// <summary>
    /// Commande pour naviguer vers Services
    /// </summary>
    [RelayCommand]
    private void NavigateToServices()
    {
        CurrentView = _serviceProvider.GetRequiredService<ServicesViewModel>();
    }
    
    /// <summary>
    /// Commande pour naviguer vers l'activation de licence
    /// </summary>
    [RelayCommand]
    public void NavigateToLicence()
    {
        var licenseViewModel = _serviceProvider.GetRequiredService<LicenseActivationViewModel>();
        
        // ✅ CORRIGÉ : Recharger les features après activation
        licenseViewModel.OnLicenseActivated = async () =>
        {
            // Recharger les features pour débloquer les boutons Pro
            await RefreshLicenseFeaturesAsync();
            
            // Retourner au Dashboard
            NavigateToDashboard();
        };
        
        var licenseView = new LicenseActivationView
        {
            DataContext = licenseViewModel
        };
        CurrentView = licenseView;
    }
    
    /// <summary>
    /// Commande pour naviguer vers la Planification (Pro uniquement)
    /// </summary>
    [RelayCommand]
    private void NavigateToPlanification()
    {
        if (IsProFeatureEnabled)
        {
            var planificationViewModel = _serviceProvider.GetRequiredService<PlanificationViewModel>();
            var planificationView = new PlanificationView
            {
                DataContext = planificationViewModel
            };
            CurrentView = planificationView;
        }
    }
    
    /// <summary>
    /// Commande pour naviguer vers l'Audit Sécurité (Pro uniquement)
    /// </summary>
    [RelayCommand]
    private void NavigateToAudit()
    {
        if (IsProFeatureEnabled)
        {
            var auditViewModel = _serviceProvider.GetRequiredService<AuditSecuriteViewModel>();
            var auditView = new AuditSecuriteView
            {
                DataContext = auditViewModel
            };
            CurrentView = auditView;
        }
    }
}