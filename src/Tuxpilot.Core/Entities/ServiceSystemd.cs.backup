namespace Tuxpilot.Core.Entities;

/// <summary>
/// Représente un service systemd
/// </summary>
public class ServiceSystemd
{
    /// <summary>
    /// Nom du service (ex: sshd, apache2)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Statut du service (active, inactive, failed)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Si le service démarre automatiquement (enabled/disabled)
    /// </summary>
    public string Enabled { get; set; } = string.Empty;
    
    /// <summary>
    /// Nom complet de l'unité systemd
    /// </summary>
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Indique si le service est actif
    /// </summary>
    public bool IsActive => Status.Equals("active", StringComparison.OrdinalIgnoreCase);
    
    /// <summary>
    /// Indique si le service est en erreur
    /// </summary>
    public bool IsFailed => Status.Equals("failed", StringComparison.OrdinalIgnoreCase);
}