namespace Tuxpilot.Core.Entities;

public class OllamaSetupAction
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public bool NeedsSudo { get; set; }
    public bool Safe { get; set; }
    public string Command { get; set; } = "";
    public string Notes { get; set; } = "";
}