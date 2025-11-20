using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Dtos;
using Tuxpilot.Infrastructure.Extensions;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Service de gestion des services systemd
/// </summary>
public class ServiceGestionServices : IServiceGestionServices
{
    private readonly ExecuteurScriptPython _executeur;
    
    public ServiceGestionServices(ExecuteurScriptPython executeur)
    {
        _executeur = executeur;
    }
    
    public async Task<List<ServiceSystemd>> ListerServicesAsync()
    {
        try
        {
            var resultat = await _executeur.ExecuterAsync("services.py", "list");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var dto = JsonSerializer.Deserialize<ServicesListDto>(resultat, options);
            
            if (dto == null || dto.Services == null)
                return new List<ServiceSystemd>();
            
            return dto.Services.Select(s => s.ToEntity()).ToList();
        }
        catch (Exception)
        {
            return new List<ServiceSystemd>();
        }
    }
    
    public async Task<(bool Success, string Message)> DemarrerServiceAsync(string serviceName)
    {
        return await ControlerServiceAsync(serviceName, "start");
    }
    
    public async Task<(bool Success, string Message)> ArreterServiceAsync(string serviceName)
    {
        return await ControlerServiceAsync(serviceName, "stop");
    }
    
    public async Task<(bool Success, string Message)> RedemarrerServiceAsync(string serviceName)
    {
        return await ControlerServiceAsync(serviceName, "restart");
    }
    
    public async Task<string> ObtenirLogsAsync(string serviceName, int lignes = 50)
    {
        try
        {
            var resultat = await _executeur.ExecuterAsync("services.py", $"logs {serviceName} {lignes}");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var response = JsonSerializer.Deserialize<JsonDocument>(resultat, options);
            
            if (response != null && response.RootElement.TryGetProperty("logs", out var logsElement))
            {
                return logsElement.GetString() ?? "Aucun log disponible";
            }
            
            return "Impossible de récupérer les logs";
        }
        catch (Exception ex)
        {
            return $"❌ Erreur : {ex.Message}";
        }
    }
    
    private async Task<(bool Success, string Message)> ControlerServiceAsync(string serviceName, string action)
    {
        try
        {
            var resultat = await _executeur.ExecuterAsync("services.py", $"control {serviceName} {action}");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            var dto = JsonSerializer.Deserialize<ServiceControlResultDto>(resultat, options);
            
            if (dto == null)
                return (false, "Erreur lors du contrôle du service");
            
            return (dto.Success, dto.Message);
        }
        catch (Exception ex)
        {
            return (false, $"Erreur : {ex.Message}");
        }
    }
}