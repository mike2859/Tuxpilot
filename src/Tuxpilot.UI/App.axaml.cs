using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Services;
using Tuxpilot.UI.ViewModels;
using Tuxpilot.UI.Views;

namespace Tuxpilot.UI;

public partial class App : Application
{
    public static ServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configuration de l'injection de dépendances
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<ExecuteurScriptPython>();
        services.AddSingleton<IServiceSysteme, ServiceSysteme>();
        services.AddSingleton<IServiceMisesAJour, ServiceMisesAJour>();
        services.AddSingleton<IServiceAssistantIA, ServiceOllama>();
        services.AddSingleton<IServiceGestionServices, ServiceGestionServices>();
        services.AddSingleton<IServiceCommandes, ServiceCommandes>();
        services.AddSingleton<IServiceHistorique, ServiceHistorique>();
        services.AddSingleton<IServiceNettoyage, ServiceNettoyage>();
        services.AddSingleton<IServiceDiagnostic, ServiceDiagnostic>();
        services.AddSingleton<IServiceTheme, ServiceTheme>(); 
        services.AddSingleton<IServiceDetectionPlanificateur, ServiceDetectionPlanificateur>();
        services.AddSingleton<IServiceSecurite, ServiceSecurite>(); 
        services.AddSingleton<ILicenseService, LicenseService>(); 
        services.AddSingleton<IServiceContexteSysteme, ServiceContexteSysteme>(); 
        services.AddSingleton<IServiceAssistantIASetup, ServiceAssistantIASetup>();
        
        // Factory de planification
        services.AddSingleton<ServicePlanificationFactory>();

        // Enregistrer IServicePlanification comme une fonction qui utilise la Factory
        // services.AddSingleton<IServicePlanification>(provider =>
        // {
        //     var factory = provider.GetRequiredService<ServicePlanificationFactory>();
        //     return factory.ObtenirServiceAsync().GetAwaiter().GetResult();
        // });
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MisesAJourViewModel>();
        services.AddTransient<NettoyageViewModel>();
        services.AddTransient<DiagnosticViewModel>();
        services.AddTransient<AssistantIAViewModel>();
        services.AddTransient<ServicesViewModel>();
        services.AddTransient<PlanificationViewModel>(); 
        services.AddTransient<AuditSecuriteViewModel>();
        services.AddTransient<LicenseActivationViewModel>();
        
        ServiceProvider = services.BuildServiceProvider();

        // 🆕 Charger le thème sauvegardé
        var serviceTheme = ServiceProvider.GetRequiredService<IServiceTheme>();
        AppliquerTheme(serviceTheme.ChargerTheme());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // 🆕 Méthode SIMPLE pour appliquer un thème
     public void AppliquerTheme(Theme theme)
    {
        if (theme == Theme.Dark)
        {
            // 🌙 THÈME SOMBRE
            Resources["BackgroundPrimary"] = new SolidColorBrush(Color.Parse("#0F172A"));
            Resources["BackgroundSecondary"] = new SolidColorBrush(Color.Parse("#1E293B"));
            Resources["BackgroundTertiary"] = new SolidColorBrush(Color.Parse("#334155"));
            
            Resources["TextPrimary"] = new SolidColorBrush(Color.Parse("#F1F5F9"));
            Resources["TextSecondary"] = new SolidColorBrush(Color.Parse("#CBD5E1"));
            Resources["TextMuted"] = new SolidColorBrush(Color.Parse("#94A3B8"));
            
            Resources["BorderPrimary"] = new SolidColorBrush(Color.Parse("#334155"));
            Resources["BorderSecondary"] = new SolidColorBrush(Color.Parse("#475569"));
            
            Resources["Primary"] = new SolidColorBrush(Color.Parse("#60A5FA"));
            Resources["PrimaryHover"] = new SolidColorBrush(Color.Parse("#3B82F6"));
            
            // 🆕 Backgrounds contextuels (Dark)
            Resources["BackgroundSuccess"] = new SolidColorBrush(Color.Parse("#064E3B"));
            Resources["BackgroundWarning"] = new SolidColorBrush(Color.Parse("#78350F"));
            Resources["BackgroundDanger"] = new SolidColorBrush(Color.Parse("#7F1D1D"));
            Resources["BackgroundInfo"] = new SolidColorBrush(Color.Parse("#1E3A8A"));
            
            // 🆕 Text contextuels (Dark - clairs)
            Resources["TextSuccess"] = new SolidColorBrush(Color.Parse("#6EE7B7"));
            Resources["TextWarning"] = new SolidColorBrush(Color.Parse("#FCD34D"));
            Resources["TextDanger"] = new SolidColorBrush(Color.Parse("#FCA5A5"));
            Resources["TextInfo"] = new SolidColorBrush(Color.Parse("#93C5FD"));
            
            // 🆕 Couleurs sémantiques
            Resources["Success"] = new SolidColorBrush(Color.Parse("#10B981"));
            Resources["Warning"] = new SolidColorBrush(Color.Parse("#F59E0B"));
            Resources["Danger"] = new SolidColorBrush(Color.Parse("#EF4444"));
            Resources["Info"] = new SolidColorBrush(Color.Parse("#3B82F6"));
            
            // 🆕 Overlays
            Resources["OverlayBackground"] = Color.Parse("#80000000");
            Resources["ShadowColor"] = Color.Parse("#60000000");
        }
        else
        {
            // ☀️ THÈME CLAIR
            Resources["BackgroundPrimary"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            Resources["BackgroundSecondary"] = new SolidColorBrush(Color.Parse("#F9FAFB"));
            Resources["BackgroundTertiary"] = new SolidColorBrush(Color.Parse("#F3F4F6"));
            
            Resources["TextPrimary"] = new SolidColorBrush(Color.Parse("#111827"));
            Resources["TextSecondary"] = new SolidColorBrush(Color.Parse("#6B7280"));
            Resources["TextMuted"] = new SolidColorBrush(Color.Parse("#9CA3AF"));
            
            Resources["BorderPrimary"] = new SolidColorBrush(Color.Parse("#E5E7EB"));
            Resources["BorderSecondary"] = new SolidColorBrush(Color.Parse("#D1D5DB"));
            
            Resources["Primary"] = new SolidColorBrush(Color.Parse("#3B82F6"));
            Resources["PrimaryHover"] = new SolidColorBrush(Color.Parse("#2563EB"));
            
            // 🆕 Backgrounds contextuels (Light)
            Resources["BackgroundSuccess"] = new SolidColorBrush(Color.Parse("#ECFDF5"));
            Resources["BackgroundWarning"] = new SolidColorBrush(Color.Parse("#FFFBEB"));
            Resources["BackgroundDanger"] = new SolidColorBrush(Color.Parse("#FEE2E2"));
            Resources["BackgroundInfo"] = new SolidColorBrush(Color.Parse("#EFF6FF"));
            
            // 🆕 Text contextuels (Light - foncés)
            Resources["TextSuccess"] = new SolidColorBrush(Color.Parse("#047857"));
            Resources["TextWarning"] = new SolidColorBrush(Color.Parse("#92400E"));
            Resources["TextDanger"] = new SolidColorBrush(Color.Parse("#991B1B"));
            Resources["TextInfo"] = new SolidColorBrush(Color.Parse("#1E40AF"));
            
            // 🆕 Couleurs sémantiques (identiques pour cohérence)
            Resources["Success"] = new SolidColorBrush(Color.Parse("#10B981"));
            Resources["Warning"] = new SolidColorBrush(Color.Parse("#F59E0B"));
            Resources["Danger"] = new SolidColorBrush(Color.Parse("#EF4444"));
            Resources["Info"] = new SolidColorBrush(Color.Parse("#3B82F6"));
            
            // 🆕 Overlays (identiques - fonctionnent sur les deux thèmes)
            Resources["OverlayBackground"] = Color.Parse("#80000000");
            Resources["ShadowColor"] = Color.Parse("#40000000");
            
            // ⚠️ ANCIEN (à supprimer - remplacé par les ressources ci-dessus)
            // Resources["SuccessTextOnBackground"] = new SolidColorBrush(Color.Parse("#065F46"));
            // Resources["WarningTextOnBackground"] = new SolidColorBrush(Color.Parse("#92400E"));
            // Resources["ErrorTextOnBackground"] = new SolidColorBrush(Color.Parse("#991B1B"));
            // Resources["InfoTextOnBackground"] = new SolidColorBrush(Color.Parse("#1E40AF"));
        }
    }
}