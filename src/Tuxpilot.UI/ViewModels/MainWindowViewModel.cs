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
    
    [ObservableProperty]
    private object? _currentView;
    
    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";

    [ObservableProperty]
    private bool _estThemeSombre;
    
    public string IconeTheme => EstThemeSombre ? "☀️" : "🌙";

    public MainWindowViewModel(IServiceProvider  serviceProvider, IServiceTheme serviceTheme)
    {
        _serviceProvider = serviceProvider;
        _serviceTheme = serviceTheme;
        
        EstThemeSombre = _serviceTheme.ThemeActuel == Theme.Dark;
        
        CurrentView = ObtenirDashboardViewModel();
    }


    private DashboardViewModel ObtenirDashboardViewModel()
    {
        var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
        var dashboardView = new DashboardView
        {
            DataContext = dashboardViewModel
        };

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
        OnPropertyChanged(nameof(IconeTheme));
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
    
    [RelayCommand]
    private void NavigateToAssistant()
    {
        CurrentView = _serviceProvider.GetRequiredService<AssistantIAViewModel>();
    }
    
    [RelayCommand]
    private void NavigateToServices()
    {
        CurrentView = _serviceProvider.GetRequiredService<ServicesViewModel>();
    }
}
