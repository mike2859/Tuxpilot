using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// Modèle pour afficher les infos système dans la vue
/// </summary>
public partial class SystemInfoViewModel : ObservableObject
{
    [ObservableProperty]
    private string _distribution = string.Empty;
    
    [ObservableProperty]
    private string _kernelVersion = string.Empty;
    
    [ObservableProperty]
    private long _totalRamMB;
    
    [ObservableProperty]
    private long _usedRamMB;
    
    [ObservableProperty]
    private double _ramPercent;
    
    [ObservableProperty]
    private double _cpuPercent;
    
    [ObservableProperty]
    private double _diskPercent;
    
    [ObservableProperty]
    private string _packageManager = string.Empty;
    
    [ObservableProperty]
    private string _cpuModel = string.Empty;

    [ObservableProperty]
    private int _cpuCores;

    [ObservableProperty]
    private int _cpuThreads;
    
    /// <summary>
    /// Texte formaté pour l'affichage de la RAM
    /// </summary>
    public string RamUsageText => $"{UsedRamMB / 1024.0:F1} GB / {TotalRamMB / 1024.0:F1} GB ({RamPercent:F1}%)";
    
    /// <summary>
    /// Texte formaté pour le CPU
    /// </summary>
    public string CpuUsageText => $"{CpuPercent:F1}%";
    
    /// <summary>
    /// Texte formaté pour le disque
    /// </summary>
    public string DiskUsageText => $"{DiskPercent:F1}%";
    
    // ✨ NOUVELLES PROPRIÉTÉS pour les couleurs
    
    /// <summary>
    /// Couleur de la barre RAM selon le pourcentage
    /// </summary>
    public string RamColor => RamPercent switch
    {
        >= 85 => "#EF4444",  // Rouge si >= 85%
        >= 70 => "#F59E0B",  // Orange si >= 70%
        _ => "#10B981"       // Vert si < 70%
    };
    
    /// <summary>
    /// Couleur de la barre CPU selon le pourcentage
    /// </summary>
    public string CpuColor => CpuPercent switch
    {
        >= 85 => "#EF4444",  // Rouge
        >= 70 => "#F59E0B",  // Orange
        _ => "#10B981"       // Vert
    };
    
    /// <summary>
    /// Couleur de la barre Disque selon le pourcentage
    /// </summary>
    public string DiskColor => DiskPercent switch
    {
        >= 85 => "#EF4444",  // Rouge
        >= 70 => "#F59E0B",  // Orange
        _ => "#10B981"       // Vert
    };
}