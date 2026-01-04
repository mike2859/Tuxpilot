namespace Tuxpilot.Core.Entities;

public class SystemContextSnapshot
{
    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;

    // Infos système
    public string OsPrettyName { get; set; } = "";
    public string Kernel { get; set; } = "";
    public string Hostname { get; set; } = "";
    public string Desktop { get; set; } = "";

    // Résumé sécurité (issu de ton audit Python -> C#)
    public int? SecurityScore { get; set; }
    public List<SecurityIssue> TopIssues { get; set; } = new();

    // Réseau (résumé)
    public List<string> ExposedPorts { get; set; } = new(); // ex: "631/cups", "5353/avahi"
    public string FirewallStatus { get; set; } = "";        // ex: "firewalld active"

    // MAJ / reboot
    public int? SecurityUpdatesCount { get; set; }
    public bool? RebootRequired { get; set; }

    // Notes / incertitudes
    public List<string> Notes { get; set; } = new();
}
