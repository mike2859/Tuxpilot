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
            RamTotaleMB = json.RamTotaleMB,
            RamUtiliseeMB = json.RamUtiliseeMB,
            RamLibreMB = json.RamLibreMB,
            PourcentageRam = json.PourcentageRam,
            PourcentageCpu = json.PourcentageCpu,
            PourcentageDisque = json.PourcentageDisque,
            GestionnairePaquets = json.GestionnairePaquets
        };
    }
    
    /// <summary>
    /// Classe pour désérialiser le JSON du script Python
    /// </summary>
    private class SystemInfoJson
    {
        public string Distribution { get; set; } = string.Empty;
        public string Kernel { get; set; } = string.Empty;
        public long RamTotaleMB { get; set; }
        public long RamUtiliseeMB { get; set; }
        public long RamLibreMB { get; set; }
        public double PourcentageRam { get; set; }
        public double PourcentageCpu { get; set; }
        public double PourcentageDisque { get; set; }
        public string GestionnairePaquets { get; set; } = string.Empty;
    }
}