using Tuxpilot.Core.Entities;

namespace Tuxpilot.UI.ViewModels;

/// <summary>
/// ViewModel pour une entrée de log avec propriétés de style
/// </summary>
public class LogEntryViewModel
{
    private readonly LogEntry _logEntry;

    public LogEntryViewModel(LogEntry logEntry)
    {
        _logEntry = logEntry;
    }

    public string Timestamp => _logEntry.Timestamp;
    public string Service => _logEntry.Service;
    public string Message => _logEntry.Message;
    public string Severity => _logEntry.Severity;

    /// <summary>
    /// Couleur de fond selon la sévérité
    /// </summary>
    public string BackgroundColor => Severity switch
    {
        "error" => "#FEE2E2",      // Rouge clair
        "warning" => "#FEF3C7",    // Jaune clair
        "info" => "#DBEAFE",       // Bleu clair
        _ => "#F3F4F6"             // Gris par défaut
    };

    /// <summary>
    /// Couleur du texte du service selon la sévérité
    /// </summary>
    public string ServiceColor => Severity switch
    {
        "error" => "#DC2626",      // Rouge foncé
        "warning" => "#D97706",    // Orange foncé
        "info" => "#1E40AF",       // Bleu foncé
        _ => "#6B7280"             // Gris
    };

    /// <summary>
    /// Couleur du texte du message (toujours foncé pour lisibilité)
    /// </summary>
    public string MessageColor => "#1F2937";  // Gris très foncé

    /// <summary>
    /// Nom de la ressource d'icône selon la sévérité
    /// </summary>
    public string IconResourceKey => Severity switch
    {
        "error" => "IconClose",      // ❌ Croix pour erreur
        "warning" => "IconWarning",  // ⚠️ Triangle pour warning
        "info" => "IconInfo",        // ℹ️ Cercle info
        _ => "IconInfo"
    };

    /// <summary>
    /// Couleur de l'icône selon la sévérité
    /// </summary>
    public string IconColor => Severity switch
    {
        "error" => "#DC2626",      // Rouge foncé
        "warning" => "#D97706",    // Orange foncé
        "info" => "#1E40AF",       // Bleu foncé
        _ => "#6B7280"             // Gris
    };
}