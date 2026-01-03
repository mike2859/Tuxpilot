using Tuxpilot.Core.Enums;

namespace Tuxpilot.Core.Entities;


/// <summary>
/// Représente les informations système collectées
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// Nom de la distribution (ex: "Fedora 41")
    /// </summary>
    public string Distribution { get; set; } = string.Empty;
    
    /// <summary>
    /// Version du kernel Linux
    /// </summary>
    public string VersionKernel { get; set; } = string.Empty;
    
    /// <summary>
    /// Mémoire RAM totale en mégaoctets
    /// </summary>
    public long RamTotaleMB { get; set; }
    
    /// <summary>
    /// Mémoire RAM utilisée en mégaoctets
    /// </summary>
    public long RamUtiliseeMB { get; set; }
    
    /// <summary>
    /// Mémoire RAM libre en mégaoctets
    /// </summary>
    public long RamLibreMB { get; set; }
    
    /// <summary>
    /// Pourcentage d'utilisation RAM
    /// </summary>
    public double PourcentageRam { get; set; }
    
    /// <summary>
    /// Pourcentage d'utilisation CPU moyen
    /// </summary>
    public double PourcentageCpu { get; set; }
    
    /// <summary>
    /// Pourcentage d'utilisation du disque
    /// </summary>
    public double PourcentageDisque { get; set; }
    
    /// <summary>
    /// Gestionnaire de paquets détecté (dnf5, dnf, apt, etc.)
    /// </summary>
    public string GestionnairePaquets { get; set; } = string.Empty;
    
    /// <summary>
    /// Modèle du processeur (ex: "Intel Core i9-14900HX")
    /// </summary>
    public string CpuModel { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de cœurs physiques
    /// </summary>
    public int CpuCores { get; set; }

    /// <summary>
    /// Nombre de threads logiques
    /// </summary>
    public int CpuThreads { get; set; }
    
    /// <summary>
    /// Évalue l'état de santé global du système
    /// </summary>
    public StatutSysteme ObtenirStatut()
    {
        if (PourcentageRam > 90 || PourcentageDisque > 90)
            return StatutSysteme.Critique;
        if (PourcentageRam > 75 || PourcentageDisque > 80)
            return StatutSysteme.Avertissement;
        return StatutSysteme.Sain;
    }
}

