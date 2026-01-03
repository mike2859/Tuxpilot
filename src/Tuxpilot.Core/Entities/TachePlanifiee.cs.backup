using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Entities;


/// <summary>
/// Représente une tâche planifiée
/// </summary>
public class TachePlanifiee
{
    /// <summary>
    /// Identifiant unique
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Type de tâche
    /// </summary>
    public TypeTache Type { get; set; }
    
    /// <summary>
    /// Nom de la tâche
    /// </summary>
    public string Nom { get; set; } = string.Empty;
    
    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Jour de la semaine (0 = Dimanche, 1 = Lundi, etc.)
    /// </summary>
    public int JourSemaine { get; set; }
    
    /// <summary>
    /// Heure d'exécution (0-23)
    /// </summary>
    public int Heure { get; set; }
    
    /// <summary>
    /// Minute d'exécution (0-59)
    /// </summary>
    public int Minute { get; set; }
    
    /// <summary>
    /// Tâche activée ou non
    /// </summary>
    public bool Activee { get; set; } = true;
    
    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Date de dernière exécution
    /// </summary>
    public DateTime? DerniereExecution { get; set; }
    
    /// <summary>
    /// Icône selon le type
    /// </summary>
    // public string Icone => Type switch
    // {
    //     TypeTache.MisesAJour => "🔄",
    //     TypeTache.Nettoyage => "🧹",
    //     TypeTache.Rapport => "📊",
    //     _ => "📋"
    // };
    
    
    /// <summary>
    /// Nom de la ressource d'icône selon le type
    /// </summary>
    public string IconeResourceKey => Type switch
    {
        TypeTache.MisesAJour => "IconSync",
        TypeTache.Nettoyage => "IconCleanup",
        TypeTache.Rapport => "IconChart",
        _ => "IconList"
    };
    
    /// <summary>
    /// Nom du jour formaté
    /// </summary>
    public string JourFormate => JourSemaine switch
    {
        -1 => "Tous les jours",
        0 => "Dimanche",
        1 => "Lundi",
        2 => "Mardi",
        3 => "Mercredi",
        4 => "Jeudi",
        5 => "Vendredi",
        6 => "Samedi",
        _ => "Inconnu"
    };
    
    /// <summary>
    /// Heure formatée
    /// </summary>
    public string HeureFormatee => $"{Heure:D2}:{Minute:D2}";
    
    /// <summary>
    /// Expression cron
    /// </summary>
    public string ExpressionCron => JourSemaine == -1 
        ? $"{Minute} {Heure} * * *"  // 🆕 Tous les jours
        : $"{Minute} {Heure} * * {JourSemaine}";
}