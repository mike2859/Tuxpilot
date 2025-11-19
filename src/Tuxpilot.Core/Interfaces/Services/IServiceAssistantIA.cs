namespace Tuxpilot.Core.Interfaces.Services;


/// <summary>
/// Service d'assistant IA basé sur LLM
/// </summary>
public interface IServiceAssistantIA
{
    /// <summary>
    /// Envoie une question à l'assistant IA et reçoit la réponse
    /// </summary>
    Task<string> DemanderAsync(string question);
    
    /// <summary>
    /// Envoie une question avec streaming de la réponse
    /// </summary>
    Task DemanderAvecStreamingAsync(string question, Action<string> onTokenReceived);
}