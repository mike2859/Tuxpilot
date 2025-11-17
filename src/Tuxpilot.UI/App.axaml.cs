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
            //var services = ConfigurerServices();
            _serviceProvider = ConfigureServices();
            
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            // Créer la fenêtre principale
           // var mainViewModel = services.GetRequiredService<MainWindowViewModel>();
           var mainViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            
            // desktop.MainWindow = new MainWindow
            // {
            //     DataContext = new MainWindowViewModel(),
            // };
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
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        
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
      
    private static ServiceProvider ConfigurerServices()
    {
        var services = new ServiceCollection();
        
        // Infrastructure
        services.AddSingleton<ExecuteurScriptPython>();
        services.AddSingleton<IServiceSysteme, ServiceSysteme>();
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        
        return services.BuildServiceProvider();
    }
}