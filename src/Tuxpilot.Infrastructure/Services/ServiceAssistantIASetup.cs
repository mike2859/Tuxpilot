using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;

public class ServiceAssistantIASetup : IServiceAssistantIASetup
{
    private const string ScriptName = "assistant_ia_setup.py";

    private readonly ExecuteurScriptPython _python;
    private readonly IServiceCommandes _commandes;

    // petit cache (évite de relancer le script à chaque refresh UI)
    private OllamaSetupStatus? _cache;
    private DateTimeOffset _cacheAt;
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(30);

    public ServiceAssistantIASetup(ExecuteurScriptPython python, IServiceCommandes commandes)
    {
        _python = python;
        _commandes = commandes;
    }

    public async Task<OllamaSetupStatus> GetStatusAsync(string model, bool forceRefresh = false)
    {
        if (!forceRefresh && _cache is not null && (DateTimeOffset.UtcNow - _cacheAt) < _ttl)
            return _cache;

        // Important : quote le modèle
        var args = string.IsNullOrWhiteSpace(model) ? "" : $"\"{EscapeArg(model)}\"";

        // Appelle ton exécuteur : python3 "Scripts/assistant_ia_setup.py" "<model>" :contentReference[oaicite:2]{index=2}
        var json = await _python.ExecuterAsync(ScriptName, args);

        var status = JsonSerializer.Deserialize<OllamaSetupStatus>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (status == null)
            throw new Exception("Réponse JSON invalide du script assistant_ia_setup.py");

        _cache = status;
        _cacheAt = DateTimeOffset.UtcNow;
        return status;
    }

    public async Task<(bool Success, string Output)> ExecuterActionAsync(OllamaSetupAction action)
    {
        if (action == null) return (false, "Action null");
        if (string.IsNullOrWhiteSpace(action.Command)) return (false, "Commande vide");

        // On exécute via ton ServiceCommandes qui sait utiliser pkexec si needsSudo=true :contentReference[oaicite:3]{index=3}
        // MAIS : ton ServiceCommandes enveloppe déjà bash -c "<command>" ; donc on lui passe une commande "simple"
        // => on échappe les guillemets pour éviter les soucis.
        var cmd = EscapeForBashC(action.Command);

        // needsSudo => pkexec ; sinon /bin/bash :contentReference[oaicite:4]{index=4}
        return await _commandes.ExecuterCommandeAsync(cmd, needsSudo: action.NeedsSudo);
    }

    private static string EscapeArg(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string EscapeForBashC(string s)
    {
        // ServiceCommandes fait: bash -c "<command>"
        // donc on doit protéger les guillemets dans <command>
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
