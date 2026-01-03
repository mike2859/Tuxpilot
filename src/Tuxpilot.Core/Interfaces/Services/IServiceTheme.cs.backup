using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service de gestion du thème de l'application
/// </summary>
public interface IServiceTheme
{
    /// <summary>
    /// Obtient le thème actuel
    /// </summary>
    Theme ThemeActuel { get; }
    
    /// <summary>
    /// Change le thème de l'application
    /// </summary>
    void ChangerTheme(Theme theme);
    
    /// <summary>
    /// Charge le thème sauvegardé
    /// </summary>
    Theme ChargerTheme();
    
    /// <summary>
    /// Sauvegarde le thème
    /// </summary>
    void SauvegarderTheme(Theme theme);
}