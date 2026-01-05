namespace Tuxpilot.Core.Entities;

public class OllamaInfo
{
    public bool Installed { get; set; }
    public string Version { get; set; } = "";
    public string ServiceActive { get; set; } = "";
    public string ServiceEnabled { get; set; } = "";
    public bool Listening11434 { get; set; }
    public List<string> Models { get; set; } = new();
}