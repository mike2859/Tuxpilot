using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;

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
        var json = JsonSerializer.Deserialize<SystemInfoJson>(resultat, options);
        
        if (json == null)
            throw new Exception("Impossible de désérialiser les infos système");
        
        // Mapper vers l'entité du domaine
        return new SystemInfo
        {
            Distribution = json.Distribution,
            VersionKernel = json.Kernel,
            CpuModel = json.CpuModel,
            CpuCores = json.CpuCores,
            CpuThreads = json.CpuThreads,
            RamTotaleMB = json.RamTotaleMB,
            RamUtiliseeMB = json.RamUtiliseeMB,
            RamLibreMB = json.RamLibreMB,
            PourcentageRam = json.PourcentageRam,
            PourcentageCpu = json.PourcentageCpu,
            PourcentageDisque = json.PourcentageDisque,
            GestionnairePaquets = json.GestionnairePaquets
        };
    }
    
  
}