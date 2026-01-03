using System.Text.Json.Serialization;

namespace Tuxpilot.UI.Models;


/// <summary>
/// Action détectée par l'IA
/// </summary>
public class AssistantAction
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
    
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;
    
    [JsonPropertyName("package")]
    public string? Package { get; set; }
    
    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
    
    [JsonPropertyName("needsSudo")]
    public bool NeedsSudo { get; set; }
    
    /// <summary>
    /// Vérifie si c'est une action valide
    /// </summary>
    public bool IsValid => Type == "action" && !string.IsNullOrEmpty(Command);
}