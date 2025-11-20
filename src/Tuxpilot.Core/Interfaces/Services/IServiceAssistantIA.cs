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
    
    Task DemanderAvecStreamingAsync(string question, Action<string> onTokenReceived);
    
    /// <summary>
    /// 🆕 Analyse l'état système et génère des suggestions proactives
    /// </summary>
    Task<string> AnalyserSystemeAsync(
        double pourcentageRam, 
        double pourcentageCpu, 
        double pourcentageDisque,
        int nombreMisesAJour);
}