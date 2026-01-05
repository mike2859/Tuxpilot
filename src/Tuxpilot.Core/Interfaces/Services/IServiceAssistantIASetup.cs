using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;

public interface IServiceAssistantIASetup
{
    Task<OllamaSetupStatus> GetStatusAsync(string model, bool forceRefresh = false);
    Task<(bool Success, string Output)> ExecuterActionAsync(OllamaSetupAction action);
}