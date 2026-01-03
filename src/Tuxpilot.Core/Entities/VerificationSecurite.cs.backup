using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Entities;


/// <summary>
/// Représente une vérification de sécurité
/// </summary>
public class VerificationSecurite
{
    /// <summary>
    /// Nom de la vérification
    /// </summary>
    public string Nom { get; set; } = string.Empty;
    
    /// <summary>
    /// Description du contrôle
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Niveau de risque détecté
    /// </summary>
    public NiveauRisque Niveau { get; set; }
    
    /// <summary>
    /// La vérification est-elle passée ?
    /// </summary>
    public bool Reussie { get; set; }
    
    /// <summary>
    /// Détails du résultat
    /// </summary>
    public string Details { get; set; } = string.Empty;
    
    /// <summary>
    /// Recommandation d'action
    /// </summary>
    public string Recommandation { get; set; } = string.Empty;
    
    /// <summary>
    /// Commande pour corriger (si applicable)
    /// </summary>
    public string? CommandeCorrection { get; set; }
    
    /// <summary>
    /// Points attribués (pour le score)
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// Points maximum possibles
    /// </summary>
    public int PointsMax { get; set; } = 20;
    
    /// <summary>
    /// Icône selon le niveau de risque
    /// </summary>
    public string Icone => Niveau switch
    {
        NiveauRisque.Aucun => "✅",
        NiveauRisque.Faible => "ℹ️",
        NiveauRisque.Moyen => "⚠️",
        NiveauRisque.Eleve => "🔴",
        NiveauRisque.Critique => "🚨",
        _ => "❓"
    };
    
    /// <summary>
    /// Couleur selon le niveau de risque
    /// </summary>
    public string Couleur => Niveau switch
    {
        NiveauRisque.Aucun => "#10B981",      // Vert
        NiveauRisque.Faible => "#3B82F6",     // Bleu
        NiveauRisque.Moyen => "#F59E0B",      // Orange
        NiveauRisque.Eleve => "#EF4444",      // Rouge
        NiveauRisque.Critique => "#DC2626",   // Rouge foncé
        _ => "#6B7280"
    };
    
    /// <summary>
    /// Texte du niveau
    /// </summary>
    public string NiveauTexte => Niveau switch
    {
        NiveauRisque.Aucun => "Sécurisé",
        NiveauRisque.Faible => "Risque faible",
        NiveauRisque.Moyen => "Risque moyen",
        NiveauRisque.Eleve => "Risque élevé",
        NiveauRisque.Critique => "Critique",
        _ => "Inconnu"
    };
}