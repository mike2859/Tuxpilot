namespace Tuxpilot.Core.Entities;

public class OllamaSetupStatus
{
    public bool Ok { get; set; }
    public bool Ready { get; set; }
    public string Endpoint { get; set; } = "";
    public string ModelRequested { get; set; } = "";
    public OllamaInfo Ollama { get; set; } = new();
    public List<OllamaSetupAction> Actions { get; set; } = new();
    public string Message { get; set; } = "";
}