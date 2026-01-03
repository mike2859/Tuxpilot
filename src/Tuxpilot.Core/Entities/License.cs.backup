using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Entities;

 /// <summary>
    /// Représente une licence Tuxpilot
    /// </summary>
    public class License
    {
        /// <summary>
        /// Clé de licence (format: EA-2024-ABCD-1234)
        /// </summary>
        public string Key { get; set; } = string.Empty;
        
        /// <summary>
        /// Type de licence
        /// </summary>
        public LicenseType Type { get; set; }
        
        /// <summary>
        /// Licence valide ou non
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Liste des fonctionnalités disponibles
        /// </summary>
        public List<string> Features { get; set; } = new();
        
        /// <summary>
        /// Message d'erreur si licence invalide
        /// </summary>
        public string? Error { get; set; }
        
        /// <summary>
        /// Licence est stockée localement
        /// </summary>
        public bool IsStored { get; set; }
        
        /// <summary>
        /// Date d'activation
        /// </summary>
        public DateTime? ActivatedAt { get; set; }
        
        /// <summary>
        /// Date d'expiration (optionnel, géré côté serveur)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// ID utilisateur (côté serveur)
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Métadonnées additionnelles (ex: is_early_access)
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
        
        /// <summary>
        /// Vérifie si une fonctionnalité est disponible
        /// </summary>
        public bool HasFeature(string feature)
        {
            return IsValid && Features.Contains(feature);
        }
        
        /// <summary>
        /// Obtient le nom d'affichage du type de licence
        /// </summary>
        public string TypeDisplayName => Type switch
        {
            LicenseType.Community => "Community",
            LicenseType.Pro => "Pro",
            LicenseType.Organization => "Organization",
            _ => "Unknown"
        };
        
        /// <summary>
        /// Vérifie si c'est une clé Early Access
        /// </summary>
        public bool IsEarlyAccess => Key.StartsWith("EA-");
    }