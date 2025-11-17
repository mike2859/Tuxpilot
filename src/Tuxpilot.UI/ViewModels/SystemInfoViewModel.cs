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
    
    /// <summary>
    /// Texte formaté pour l'affichage de la RAM
    /// </summary>
    public string RamUsageText => $"{UsedRamMB / 1024:F1} GB / {TotalRamMB / 1024:F1} GB ({RamPercent:F1}%)";
    
    /// <summary>
    /// Texte formaté pour le CPU
    /// </summary>
    public string CpuUsageText => $"{CpuPercent:F1}%";
    
    /// <summary>
    /// Texte formaté pour le disque
    /// </summary>
    public string DiskUsageText => $"{DiskPercent:F1}%";
}