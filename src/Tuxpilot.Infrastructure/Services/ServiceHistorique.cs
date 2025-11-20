using System.Text.Json;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Service de gestion de l'historique des actions
/// </summary>
public class ServiceHistorique : IServiceHistorique
{
    private readonly string _historiqueFile;
    private const int MaxActions = 100; // Limiter à 100 actions max
    
    public ServiceHistorique()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".tuxpilot"
        );
        
        Directory.CreateDirectory(appDataPath);
        _historiqueFile = Path.Combine(appDataPath, "historique.json");
    }
    
    public async Task AjouterActionAsync(TypeAction type, string description, bool success = true)
    {
        try
        {
            var actions = await ChargerHistoriqueAsync();
            
            actions.Insert(0, new ActionHistorique
            {
                Date = DateTime.Now,
                Type = type,
                Description = description,
                Success = success
            });
            
            // Limiter le nombre d'actions
            if (actions.Count > MaxActions)
            {
                actions = actions.Take(MaxActions).ToList();
            }
            
            await SauvegarderHistoriqueAsync(actions);
        }
        catch
        {
            // Ignorer les erreurs de logging
        }
    }
    
    public async Task<List<ActionHistorique>> ObtenirDernieresActionsAsync(int count = 10)
    {
        try
        {
            var actions = await ChargerHistoriqueAsync();
            return actions.Take(count).ToList();
        }
        catch
        {
            return new List<ActionHistorique>();
        }
    }
    
    private async Task<List<ActionHistorique>> ChargerHistoriqueAsync()
    {
        if (!File.Exists(_historiqueFile))
            return new List<ActionHistorique>();
        
        try
        {
            var json = await File.ReadAllTextAsync(_historiqueFile);
            return JsonSerializer.Deserialize<List<ActionHistorique>>(json) 
                   ?? new List<ActionHistorique>();
        }
        catch
        {
            return new List<ActionHistorique>();
        }
    }
    
    private async Task SauvegarderHistoriqueAsync(List<ActionHistorique> actions)
    {
        var json = JsonSerializer.Serialize(actions, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(_historiqueFile, json);
    }
}