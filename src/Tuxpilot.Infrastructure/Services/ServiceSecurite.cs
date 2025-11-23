using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Service d'audit de sécurité
/// </summary>
public class ServiceSecurite : IServiceSecurite
{
    private readonly ExecuteurScriptPython _executeur;
    private readonly IServiceCommandes _serviceCommandes;

    public ServiceSecurite(ExecuteurScriptPython executeur, IServiceCommandes serviceCommandes)
    {
        _executeur = executeur;
        _serviceCommandes = serviceCommandes;
    }

    /// <summary>
    /// Exécuter un audit complet de sécurité
    /// </summary>
    public async Task<RapportSecurite> ExecuterAuditAsync()
    {
        try
        {
            Console.WriteLine("[SÉCURITÉ] Début de l'audit...");

            // Exécuter le script Python
            var jsonResult = await _executeur.ExecuterAsync("audit_securite.py");

            if (string.IsNullOrEmpty(jsonResult))
            {
                Console.WriteLine("[SÉCURITÉ] Erreur : Aucun résultat du script");
                return new RapportSecurite();
            }

            // Parser le JSON
            var json = JsonDocument.Parse(jsonResult);
            var root = json.RootElement;

            var rapport = new RapportSecurite();

            // Parser les vérifications
            if (root.TryGetProperty("verifications", out var verificationsElement))
            {
                foreach (var verifJson in verificationsElement.EnumerateArray())
                {
                    var verif = ParseVerification(verifJson);
                    rapport.Verifications.Add(verif);
                }
            }

            Console.WriteLine($"[SÉCURITÉ] Audit terminé - Score: {rapport.Score}/100");
            return rapport;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SÉCURITÉ] Erreur audit : {ex.Message}");
            return new RapportSecurite();
        }
    }

    /// <summary>
    /// Parser une vérification depuis JSON
    /// </summary>
    private VerificationSecurite ParseVerification(System.Text.Json.JsonElement json)
    {
        var verif = new VerificationSecurite();

        if (json.TryGetProperty("nom", out var nom))
            verif.Nom = nom.GetString() ?? "";

        if (json.TryGetProperty("description", out var desc))
            verif.Description = desc.GetString() ?? "";

        if (json.TryGetProperty("reussie", out var reussie))
            verif.Reussie = reussie.GetBoolean();

        if (json.TryGetProperty("niveau", out var niveau))
        {
            var niveauStr = niveau.GetString() ?? "Moyen";
            verif.Niveau = niveauStr switch
            {
                "Aucun" => NiveauRisque.Aucun,
                "Faible" => NiveauRisque.Faible,
                "Moyen" => NiveauRisque.Moyen,
                "Eleve" => NiveauRisque.Eleve,
                "Critique" => NiveauRisque.Critique,
                _ => NiveauRisque.Moyen
            };
        }

        if (json.TryGetProperty("points", out var points))
            verif.Points = points.GetInt32();

        if (json.TryGetProperty("points_max", out var pointsMax))
            verif.PointsMax = pointsMax.GetInt32();

        if (json.TryGetProperty("details", out var details))
            verif.Details = details.GetString() ?? "";

        if (json.TryGetProperty("recommandation", out var reco))
            verif.Recommandation = reco.GetString() ?? "";

        if (json.TryGetProperty("commande_correction", out var cmd) && cmd.ValueKind != System.Text.Json.JsonValueKind.Null)
            verif.CommandeCorrection = cmd.GetString();

        return verif;
    }

    /// <summary>
    /// Vérifier l'état du firewall
    /// </summary>
    public async Task<VerificationSecurite> VerifierFirewallAsync()
    {
        var rapport = await ExecuterAuditAsync();
        return rapport.Verifications.FirstOrDefault(v => v.Nom == "Firewall") 
            ?? new VerificationSecurite { Nom = "Firewall", Description = "Erreur" };
    }

    /// <summary>
    /// Vérifier la configuration SSH
    /// </summary>
    public async Task<VerificationSecurite> VerifierSSHAsync()
    {
        var rapport = await ExecuterAuditAsync();
        return rapport.Verifications.FirstOrDefault(v => v.Nom == "Configuration SSH")
            ?? new VerificationSecurite { Nom = "Configuration SSH", Description = "Erreur" };
    }

    /// <summary>
    /// Vérifier les mises à jour de sécurité
    /// </summary>
    public async Task<VerificationSecurite> VerifierMisesAJourSecuriteAsync()
    {
        var rapport = await ExecuterAuditAsync();
        return rapport.Verifications.FirstOrDefault(v => v.Nom == "Mises à jour de sécurité")
            ?? new VerificationSecurite { Nom = "Mises à jour de sécurité", Description = "Erreur" };
    }

    /// <summary>
    /// Vérifier les ports ouverts
    /// </summary>
    public async Task<VerificationSecurite> VerifierPortsOuvertsAsync()
    {
        var rapport = await ExecuterAuditAsync();
        return rapport.Verifications.FirstOrDefault(v => v.Nom == "Ports ouverts")
            ?? new VerificationSecurite { Nom = "Ports ouverts", Description = "Erreur" };
    }

    /// <summary>
    /// Vérifier les permissions des fichiers sensibles
    /// </summary>
    public async Task<VerificationSecurite> VerifierPermissionsFichiersAsync()
    {
        var rapport = await ExecuterAuditAsync();
        return rapport.Verifications.FirstOrDefault(v => v.Nom == "Permissions fichiers")
            ?? new VerificationSecurite { Nom = "Permissions fichiers", Description = "Erreur" };
    }

    /// <summary>
    /// Appliquer une correction automatique
    /// </summary>
    public async Task<bool> AppliquerCorrectionAsync(string commande)
    {
        try
        {
            Console.WriteLine($"[SÉCURITÉ] Application correction : {commande}");
            var result = await _serviceCommandes.ExecuterCommandeAsync(commande);
            return result.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SÉCURITÉ] Erreur correction : {ex.Message}");
            return false;
        }
    }
}