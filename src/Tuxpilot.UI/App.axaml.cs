using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Services;
using Tuxpilot.UI.ViewModels;
using Tuxpilot.UI.Views;

namespace Tuxpilot.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Configuration Dependency Injection
            _serviceProvider = ConfigureServices();
            
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            DisableAvaloniaDataAnnotationValidation();
            
            // Créer la fenêtre principale
            var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    /// <summary>
    /// Configure tous les services de l'application
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Services Infrastructure
        services.AddSingleton<ExecuteurScriptPython>();
        services.AddSingleton<IServiceSysteme, ServiceSysteme>();
        services.AddSingleton<IServiceMisesAJour, ServiceMisesAJour>();
        services.AddSingleton<IServiceNettoyage, ServiceNettoyage>();
        services.AddSingleton<IServiceDiagnostic, ServiceDiagnostic>();  
        services.AddSingleton<IServiceAssistantIA, ServiceOllama>(); 
        services.AddSingleton<IServiceGestionServices, ServiceGestionServices>();
        services.AddSingleton<IServiceCommandes, ServiceCommandes>(); 
        services.AddSingleton<IServiceHistorique, ServiceHistorique>(); 
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MisesAJourViewModel>();
        services.AddTransient<NettoyageViewModel>();
        services.AddTransient<DiagnosticViewModel>();  
        services.AddTransient<AssistantIAViewModel>(); 
        services.AddTransient<ServicesViewModel>();
        
        return services.BuildServiceProvider();
    }
    
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}