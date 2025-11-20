using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour la page de gestion des services
/// </summary>
public partial class ServicesViewModel : ViewModelBase
{
    private readonly IServiceGestionServices _serviceGestion;
    private readonly IServiceHistorique _serviceHistorique;
    
    [ObservableProperty]
    private ObservableCollection<ServiceViewModel> _services = new();
    
    [ObservableProperty]
    private ServiceViewModel? _serviceSelectionne;
    
    [ObservableProperty]
    private string _logs = string.Empty;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isLoadingLogs;
    
    [ObservableProperty]
    private bool _isExecutingAction;
    
    [ObservableProperty]
    private string _messageStatut = string.Empty;
    
    public ServicesViewModel(IServiceGestionServices serviceGestion,  IServiceHistorique serviceHistorique)
    {
        _serviceGestion = serviceGestion;
        _serviceHistorique = serviceHistorique;
        
        // Charger au démarrage
        _ = ChargerServicesAsync();
    }
    
    /// <summary>
    /// Charge la liste des services
    /// </summary>
    [RelayCommand]
    private async Task ChargerServicesAsync()
    {
        try
        {
            IsLoading = true;
            MessageStatut = "Chargement des services...";
            
            var services = await _serviceGestion.ListerServicesAsync();
            
            Services.Clear();
            foreach (var service in services.OrderBy(s => s.Name))
            {
                Services.Add(new ServiceViewModel
                {
                    Name = service.Name,
                    Status = service.Status,
                    Enabled = service.Enabled,
                    Unit = service.Unit
                });
            }
            
            MessageStatut = $"✅ {Services.Count} service(s) détecté(s)";
        }
        catch (Exception ex)
        {
            MessageStatut = $"❌ Erreur : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Sélectionne un service et charge ses logs
    /// </summary>
    [RelayCommand]
    private async Task SelectionnerServiceAsync(ServiceViewModel service)
    {
        ServiceSelectionne = service;
        await ChargerLogsAsync();
    }
    
    /// <summary>
    /// Charge les logs du service sélectionné
    /// </summary>
    [RelayCommand]
    private async Task ChargerLogsAsync()
    {
        if (ServiceSelectionne == null) return;
        
        try
        {
            IsLoadingLogs = true;
            Logs = "Chargement des logs...";
            
            var logs = await _serviceGestion.ObtenirLogsAsync(ServiceSelectionne.Name, 100);
            Logs = logs;
            
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Service,
                $"Commande ObtenirLogs exécutée : {Logs}",
                true
            );
        }
        catch (Exception ex)
        {
            Logs = $"❌ Erreur : {ex.Message}";
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Service,
                $"Échec ObtenirLogs : {Logs}",
                false
            );
        }
        finally
        {
            IsLoadingLogs = false;
        }
    }
    
    /// <summary>
    /// Démarre le service sélectionné
    /// </summary>
    [RelayCommand]
    private async Task DemarrerServiceAsync()
    {
        if (ServiceSelectionne == null) return;
        
        try
        {
            IsExecutingAction = true;
            MessageStatut = $"Démarrage de {ServiceSelectionne.Name}...";
            
            var (success, message) = await _serviceGestion.DemarrerServiceAsync(ServiceSelectionne.Name);
            
            MessageStatut = success ? $"✅ {message}" : $"❌ {message}";
            
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Service,
                $"Commande DemarrerServiceAsync exécutée : {MessageStatut}",
                true
            );
            
            // Rafraîchir la liste
            await Task.Delay(1000);
            await ChargerServicesAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"❌ Erreur : {ex.Message}";
            await _serviceHistorique.AjouterActionAsync(
                TypeAction.Service,
                $"Échec DemarrerServiceAsync : {MessageStatut}",
                false
            );
        }
        finally
        {
            IsExecutingAction = false;
        }
    }
    
    /// <summary>
    /// Arrête le service sélectionné
    /// </summary>
    [RelayCommand]
    private async Task ArreterServiceAsync()
    {
        if (ServiceSelectionne == null) return;
        
        try
        {
            IsExecutingAction = true;
            MessageStatut = $"Arrêt de {ServiceSelectionne.Name}...";
            
            var (success, message) = await _serviceGestion.ArreterServiceAsync(ServiceSelectionne.Name);
            
            MessageStatut = success ? $"✅ {message}" : $"❌ {message}";
            
            // Rafraîchir la liste
            await Task.Delay(1000);
            await ChargerServicesAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"❌ Erreur : {ex.Message}";
        }
        finally
        {
            IsExecutingAction = false;
        }
    }
    
    /// <summary>
    /// Redémarre le service sélectionné
    /// </summary>
    [RelayCommand]
    private async Task RedemarrerServiceAsync()
    {
        if (ServiceSelectionne == null) return;
        
        try
        {
            IsExecutingAction = true;
            MessageStatut = $"Redémarrage de {ServiceSelectionne.Name}...";
            
            var (success, message) = await _serviceGestion.RedemarrerServiceAsync(ServiceSelectionne.Name);
            
            MessageStatut = success ? $"✅ {message}" : $"❌ {message}";
            
            // Rafraîchir la liste
            await Task.Delay(1000);
            await ChargerServicesAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"❌ Erreur : {ex.Message}";
        }
        finally
        {
            IsExecutingAction = false;
        }
    }
}