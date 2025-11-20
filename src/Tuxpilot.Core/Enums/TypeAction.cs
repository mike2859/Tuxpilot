namespace Tuxpilot.Core.Enums;


/// <summary>
/// Type d'action dans l'historique
/// </summary>
public enum TypeAction
{
    /// <summary>
    /// Mise à jour système
    /// </summary>
    Update,
    
    /// <summary>
    /// Installation de paquet
    /// </summary>
    Install,
    
    /// <summary>
    /// Suppression de paquet
    /// </summary>
    Remove,
    
    /// <summary>
    /// Nettoyage système
    /// </summary>
    Clean,
    
    /// <summary>
    /// Gestion de service systemd
    /// </summary>
    Service,
    
    /// <summary>
    /// Action exécutée par l'IA
    /// </summary>
    AI,
    
    /// <summary>
    /// Diagnostic système
    /// </summary>
    Diagnostic
}