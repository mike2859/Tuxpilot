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

    [ObservableProperty]
    private object? _currentView;
    
    public MainWindowViewModel(IServiceProvider  serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
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
}
