namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service pour exécuter des commandes système
/// </summary>
public interface IServiceCommandes
{
    /// <summary>
    /// Exécute une commande avec pkexec si nécessaire
    /// </summary>
    Task<(bool Success, string Output)> ExecuterCommandeAsync(string command, bool needsSudo = true);
    
    /// <summary>
    /// Exécute une commande avec streaming des logs
    /// </summary>
    Task ExecuterCommandeAvecLogsAsync(string command, Action<string> onLogReceived, bool needsSudo = true);
}