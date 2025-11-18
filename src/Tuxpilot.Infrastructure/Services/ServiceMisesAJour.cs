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
}