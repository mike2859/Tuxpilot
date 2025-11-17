namespace Tuxpilot.Core.Entities;

/// <summary>
/// Classe pour désérialiser le JSON du script Python
/// </summary>

public class SystemInfoJson
{
    public string Distribution { get; set; } = string.Empty;
    public string Kernel { get; set; } = string.Empty;
        
    public string CpuModel { get; set; } = string.Empty;
    public int CpuCores { get; set; }
    public int CpuThreads { get; set; }
    public long RamTotaleMB { get; set; }
    public long RamUtiliseeMB { get; set; }
    public long RamLibreMB { get; set; }
    public double PourcentageRam { get; set; }
    public double PourcentageCpu { get; set; }
    public double PourcentageDisque { get; set; }
    public string GestionnairePaquets { get; set; } = string.Empty;
}