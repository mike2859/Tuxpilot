namespace Tuxpilot.Core.Entities;


/// <summary>
/// Informations de diagnostic système
/// </summary>
public class DiagnosticInfo
{
    public string Timestamp { get; set; } = string.Empty;
    public int ScoreSante { get; set; }
    public string EtatGlobal { get; set; } = string.Empty;
    public string MessageGlobal { get; set; } = string.Empty;
    
    // Services
    public int NombreServicesErreur { get; set; }
    public List<ServiceInfo> Services { get; set; } = new();
    
    // Logs
    public int NombreLogs { get; set; }
    public List<LogEntry> Logs { get; set; } = new();
    
    // Disque
    public DiskInfo Disque { get; set; } = new();
    
    // Processus
    public List<ProcessInfo> TopCpu { get; set; } = new();
    public List<ProcessInfo> TopRam { get; set; } = new();
    
    public string? Erreur { get; set; }
}