using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Dtos;
using Tuxpilot.Infrastructure.Extensions;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Implémentation du service de nettoyage
/// </summary>
public class ServiceNettoyage : IServiceNettoyage
{
    private readonly ExecuteurScriptPython _executeurScript;
    
    public ServiceNettoyage(ExecuteurScriptPython executeurScript)
    {
        _executeurScript = executeurScript;
    }
    
    public async Task<CleanupInfo> AnalyserNettoyageAsync()
    {
        try
        {
            // Exécuter le script Python
            var resultat = await _executeurScript.ExecuterAsync("cleanup.py");
            
            // Options de désérialisation
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };
            
            // Désérialiser le JSON
            var dto = JsonSerializer.Deserialize<CleanupInfoDto>(resultat, options);
            
            if (dto == null)
                throw new Exception("Impossible de désérialiser les infos de nettoyage");
            
            // Mapper vers l'entité
            return dto.ToEntity();
        }
        catch (Exception ex)
        {
            return new CleanupInfo
            {
                Gestionnaire = "unknown",
                Elements = new List<CleanupElement>(),
                TailleTotaleMB = 0,
                NombreElements = 0,
                Erreur = $"Erreur lors de l'analyse : {ex.Message}"
            };
        }
    }
}