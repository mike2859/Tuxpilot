using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.UI.Views;

namespace Tuxpilot.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? _currentView;
    
    public MainWindowViewModel()
    {
        // Afficher le Dashboard par défaut
        CurrentView = new DashboardView();
    }
    
    /// <summary>
    /// Commande pour naviguer vers le Dashboard
    /// </summary>
    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentView = new DashboardView();
    }
    
    /// <summary>
    /// Commande pour naviguer vers les Mises à jour
    /// </summary>
    [RelayCommand]
    private void NavigateToUpdates()
    {
        CurrentView = new MisesAJourView();
    }
    
    /// <summary>
    /// Commande pour naviguer vers le Nettoyage
    /// </summary>
    [RelayCommand]
    private void NavigateToCleanup()
    {
        CurrentView = new NettoyageView();
    }
    
    /// <summary>
    /// Commande pour naviguer vers le Diagnostic
    /// </summary>
    [RelayCommand]
    private void NavigateToDiagnostic()
    {
        CurrentView = new DiagnosticView();
    }
}
