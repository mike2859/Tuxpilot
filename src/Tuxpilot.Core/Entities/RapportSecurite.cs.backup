namespace Tuxpilot.Core.Entities;

/// <summary>
/// Rapport d'audit de sécurité
/// </summary>
public class RapportSecurite
{
    /// <summary>
    /// Date de l'audit
    /// </summary>
    public DateTime DateAudit { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Liste des vérifications effectuées
    /// </summary>
    public List<VerificationSecurite> Verifications { get; set; } = new();
    
    /// <summary>
    /// Score global de sécurité /100
    /// </summary>
    public int Score => Verifications.Any() 
        ? (int)((double)Verifications.Sum(v => v.Points) / Verifications.Sum(v => v.PointsMax) * 100)
        : 0;
    
    /// <summary>
    /// Nombre de vérifications réussies
    /// </summary>
    public int VerificationsReussies => Verifications.Count(v => v.Reussie);
    
    /// <summary>
    /// Nombre total de vérifications
    /// </summary>
    public int TotalVerifications => Verifications.Count;
    
    /// <summary>
    /// Nombre de problèmes critiques
    /// </summary>
    public int ProblemesCritiques => Verifications.Count(v => v.Niveau == Enums.NiveauRisque.Critique);
    
    /// <summary>
    /// Nombre de problèmes élevés
    /// </summary>
    public int ProblemesEleves => Verifications.Count(v => v.Niveau == Enums.NiveauRisque.Eleve);
    
    /// <summary>
    /// Nombre de problèmes moyens
    /// </summary>
    public int ProblemesMoyens => Verifications.Count(v => v.Niveau == Enums.NiveauRisque.Moyen);
    
    /// <summary>
    /// Évaluation globale
    /// </summary>
    public string Evaluation => Score switch
    {
        >= 90 => "Excellent",
        >= 75 => "Bon",
        >= 60 => "Moyen",
        >= 40 => "Faible",
        _ => "Critique"
    };
    
    /// <summary>
    /// Couleur du score
    /// </summary>
    public string CouleurScore => Score switch
    {
        >= 80 => "#10B981",  // Vert
        >= 60 => "#F59E0B",  // Orange
        _ => "#EF4444"       // Rouge
    };
    
    /// <summary>
    /// Icône selon le score
    /// </summary>
    public string Icone => Score switch
    {
        >= 90 => "🛡️",
        >= 75 => "✅",
        >= 60 => "⚠️",
        >= 40 => "🔴",
        _ => "🚨"
    };
}