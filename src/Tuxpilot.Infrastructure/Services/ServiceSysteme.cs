using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Dtos;
using Tuxpilot.Infrastructure.Extensions;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Implémentation du service système
/// </summary>
public class ServiceSysteme : IServiceSysteme
{
    private readonly ExecuteurScriptPython _executeurScript;
    
    public ServiceSysteme(ExecuteurScriptPython executeurScript)
    {
        _executeurScript = executeurScript;
    }
    
    public async Task<SystemInfo> ObtenirInfoSystemeAsync()
    {
        // Exécuter le script Python
        var resultat = await _executeurScript.ExecuterAsync("system_info.py");
        
        // Options de désérialisation (insensible à la casse)
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };
        
        // Désérialiser le JSON
        var dto = JsonSerializer.Deserialize<SystemInfoDto>(resultat, options);
        
        if (dto == null)
            throw new Exception("Impossible de désérialiser les infos système");

        return dto.ToEntity();
    }
    
  
}