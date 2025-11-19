using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Dtos;
using Tuxpilot.Infrastructure.Extensions;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Implémentation du service de diagnostic
/// </summary>
public class ServiceDiagnostic : IServiceDiagnostic
{
    private readonly ExecuteurScriptPython _executeurScript;
    
    public ServiceDiagnostic(ExecuteurScriptPython executeurScript)
    {
        _executeurScript = executeurScript;
    }
    
    public async Task<DiagnosticInfo> DiagnostiquerAsync()
    {
        try
        {
            // Exécuter le script Python
            var resultat = await _executeurScript.ExecuterAsync("diagnostic.py");
            
            // Options de désérialisation
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            // Désérialiser le JSON
            var dto = JsonSerializer.Deserialize<DiagnosticInfoDto>(resultat, options);
            
            if (dto == null)
                throw new Exception("Impossible de désérialiser les infos de diagnostic");
            
            // Mapper vers l'entité
            return dto.ToEntity();
        }
        catch (Exception ex)
        {
            return new DiagnosticInfo
            {
                Timestamp = DateTime.Now.ToString("o"),
                ScoreSante = 0,
                EtatGlobal = "erreur",
                MessageGlobal = "Erreur lors du diagnostic",
                NombreServicesErreur = 0,
                Services = new List<ServiceInfo>(),
                NombreLogs = 0,
                Logs = new List<LogEntry>(),
                Disque = new DiskInfo(),
                TopCpu = new List<ProcessInfo>(),
                TopRam = new List<ProcessInfo>(),
                Erreur = $"Erreur lors du diagnostic : {ex.Message}"
            };
        }
    }
}