using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;

/// <summary>
/// Service d'audit de sécurité
/// </summary>
public interface IServiceSecurite
{
    /// <summary>
    /// Exécuter un audit complet de sécurité
    /// </summary>
    Task<RapportSecurite> ExecuterAuditAsync();
    
    /// <summary>
    /// Vérifier l'état du firewall
    /// </summary>
    Task<VerificationSecurite> VerifierFirewallAsync();
    
    /// <summary>
    /// Vérifier la configuration SSH
    /// </summary>
    Task<VerificationSecurite> VerifierSSHAsync();
    
    /// <summary>
    /// Vérifier les mises à jour de sécurité
    /// </summary>
    Task<VerificationSecurite> VerifierMisesAJourSecuriteAsync();
    
    /// <summary>
    /// Vérifier les ports ouverts
    /// </summary>
    Task<VerificationSecurite> VerifierPortsOuvertsAsync();
    
    /// <summary>
    /// Vérifier les permissions des fichiers sensibles
    /// </summary>
    Task<VerificationSecurite> VerifierPermissionsFichiersAsync();
    
    /// <summary>
    /// Appliquer une correction automatique
    /// </summary>
    Task<bool> AppliquerCorrectionAsync(string commande);
}