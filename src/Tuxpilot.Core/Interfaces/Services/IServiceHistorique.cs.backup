using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service de gestion de l'historique des actions
/// </summary>
public interface IServiceHistorique
{
    /// <summary>
    /// Enregistre une action dans l'historique
    /// </summary>
    Task AjouterActionAsync(TypeAction type, string description, bool success = true);
    
    /// <summary>
    /// Récupère les dernières actions
    /// </summary>
    Task<List<ActionHistorique>> ObtenirDernieresActionsAsync(int count = 10);
}

/// <summary>
/// Représente une action dans l'historique
/// </summary>
public class ActionHistorique
{
    public DateTime Date { get; set; }
    public TypeAction Type { get; set; } // 🆕 TypeAction au lieu de string
    public string Description { get; set; } = string.Empty;
    public bool Success { get; set; }
    
    public string Icone => Type switch
    {
        TypeAction.Update => "🔄",
        TypeAction.Install => "📦",
        TypeAction.Remove => "🗑️",
        TypeAction.Clean => "🧹",
        TypeAction.Service => "⚙️",
        TypeAction.AI => "🤖",
        TypeAction.Diagnostic => "🔍",
        _ => "📋"
    };
    
    public string DateFormatee => Date.ToString("dd/MM/yyyy HH:mm");
}