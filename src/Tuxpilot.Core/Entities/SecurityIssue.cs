
namespace Tuxpilot.Core.Entities;


public class SecurityIssue
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Level { get; set; } = "";  // "Critique|Eleve|Moyen|Faible|Aucun"
    public string Details { get; set; } = "";
    public string Proof { get; set; } = "";
    public string Impact { get; set; } = "";
    public string Recommendation { get; set; } = "";
}
