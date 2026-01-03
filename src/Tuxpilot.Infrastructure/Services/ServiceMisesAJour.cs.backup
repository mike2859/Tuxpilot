using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Dtos;
using Tuxpilot.Infrastructure.Extensions;

namespace Tuxpilot.Infrastructure.Services;



/// <summary>
/// Implémentation du service de mises à jour
/// </summary>
public class ServiceMisesAJour : IServiceMisesAJour
{
    private readonly ExecuteurScriptPython _executeurScript;
    
    public ServiceMisesAJour(ExecuteurScriptPython executeurScript)
    {
        _executeurScript = executeurScript;
    }
    
    public async Task<UpdateInfo> VerifierMisesAJourAsync()
    {
        try
        {
            // Exécuter le script Python
            var resultat = await _executeurScript.ExecuterAsync("check_updates.py");
            
            // Options de désérialisation (insensible à la casse)
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            // Désérialiser le JSON
            var dto = JsonSerializer.Deserialize<UpdateInfoDto>(resultat, options);
            
            if (dto == null)
                throw new Exception("Impossible de désérialiser les infos de mise à jour");
            
            // Mapper vers l'entité du domaine
            return dto.ToEntity();
        }
        catch (Exception ex)
        {
            return new UpdateInfo
            {
                Gestionnaire = "unknown",
                Nombre = 0,
                Paquets = new List<Package>(),
                Erreur = $"Erreur lors de la vérification : {ex.Message}"
            };
        }
    } 
    
    public async Task<(bool Success, string Message)> InstallerMisesAJourAsync(Action<string, string>? onLogReceived = null)
    {
        try
        {
            var success = false;
            var finalMessage = "";
            
            // Exécuter le script avec streaming
            await _executeurScript.ExecuterAvecStreamingAsync(
                "install_updates.py",
                (line) =>
                {
                    try
                    {
                        // Parser le JSON de chaque ligne
                        var options = new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        };
                        
                        var log = JsonSerializer.Deserialize<InstallLogDto>(line, options);
                        
                        if (log != null)
                        {
                            // Appeler le callback si fourni
                            onLogReceived?.Invoke(log.Type, log.Message);
                            
                            // Détecter la fin
                            if (log.Type == "final_success")
                            {
                                success = true;
                                finalMessage = log.Message;
                            }
                            else if (log.Type == "error")
                            {
                                finalMessage = log.Message;
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Ligne non-JSON, on l'ignore ou on la passe telle quelle
                        onLogReceived?.Invoke("info", line);
                    }
                }
            );
            
            return (success, finalMessage.Length > 0 ? finalMessage : "Installation terminée");
        }
        catch (Exception ex)
        {
            return (false, $"Erreur lors de l'installation : {ex.Message}");
        }
    }
}