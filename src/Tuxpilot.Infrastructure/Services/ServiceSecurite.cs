using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Service d'audit de sécurité
/// </summary>
public class ServiceSecurite : IServiceSecurite
{
    private readonly IServiceCommandes _serviceCommandes;

    public ServiceSecurite(IServiceCommandes serviceCommandes)
    {
        _serviceCommandes = serviceCommandes;
    }

    /// <summary>
    /// Exécuter un audit complet de sécurité
    /// </summary>
    public async Task<RapportSecurite> ExecuterAuditAsync()
    {
        var rapport = new RapportSecurite();

        try
        {
            Console.WriteLine("[SÉCURITÉ] Début de l'audit...");

            // Exécuter toutes les vérifications
            rapport.Verifications.Add(await VerifierFirewallAsync());
            rapport.Verifications.Add(await VerifierSSHAsync());
            rapport.Verifications.Add(await VerifierMisesAJourSecuriteAsync());
            rapport.Verifications.Add(await VerifierPortsOuvertsAsync());
            rapport.Verifications.Add(await VerifierPermissionsFichiersAsync());

            Console.WriteLine($"[SÉCURITÉ] Audit terminé - Score: {rapport.Score}/100");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SÉCURITÉ] Erreur audit : {ex.Message}");
        }

        return rapport;
    }

    /// <summary>
    /// Vérifier l'état du firewall
    /// </summary>
    public async Task<VerificationSecurite> VerifierFirewallAsync()
    {
        var verif = new VerificationSecurite
        {
            Nom = "Firewall",
            Description = "Vérification de l'état du pare-feu système",
            PointsMax = 20
        };

        try
        {
            // Vérifier UFW
            var resultUfw = await _serviceCommandes.ExecuterCommandeAsync("systemctl is-active ufw");
            var ufwActif = resultUfw.Output.Trim() == "active";

            // Vérifier firewalld
            var resultFirewalld = await _serviceCommandes.ExecuterCommandeAsync("systemctl is-active firewalld");
            var firewalldActif = resultFirewalld.Output.Trim() == "active";

            if (ufwActif || firewalldActif)
            {
                verif.Reussie = true;
                verif.Niveau = NiveauRisque.Aucun;
                verif.Points = 20;
                verif.Details = ufwActif ? "UFW est actif et configuré" : "Firewalld est actif et configuré";
                verif.Recommandation = "✅ Votre pare-feu est correctement activé";
            }
            else
            {
                verif.Reussie = false;
                verif.Niveau = NiveauRisque.Eleve;
                verif.Points = 5;
                verif.Details = "Aucun pare-feu actif détecté";
                verif.Recommandation = "Activez UFW ou firewalld pour protéger votre système";
                verif.CommandeCorrection = "sudo ufw enable";
            }
        }
        catch (Exception ex)
        {
            verif.Reussie = false;
            verif.Niveau = NiveauRisque.Moyen;
            verif.Points = 10;
            verif.Details = $"Impossible de vérifier : {ex.Message}";
            verif.Recommandation = "Vérifiez manuellement l'état de votre pare-feu";
        }

        return verif;
    }

    /// <summary>
    /// Vérifier la configuration SSH
    /// </summary>
    public async Task<VerificationSecurite> VerifierSSHAsync()
    {
        var verif = new VerificationSecurite
        {
            Nom = "Configuration SSH",
            Description = "Vérification de la sécurité SSH",
            PointsMax = 20
        };

        try
        {
            var result = await _serviceCommandes.ExecuterCommandeAsync("cat /etc/ssh/sshd_config 2>/dev/null || echo 'error'");
            
            if (result.Output.Contains("error"))
            {
                verif.Reussie = true;
                verif.Niveau = NiveauRisque.Aucun;
                verif.Points = 20;
                verif.Details = "SSH n'est pas installé";
                verif.Recommandation = "✅ Aucun risque SSH (service non installé)";
                return verif;
            }

            var config = result.Output;
            var problemes = new List<string>();
            int pointsPerdus = 0;

            // Vérifier PermitRootLogin
            if (config.Contains("PermitRootLogin yes") && !config.Contains("#PermitRootLogin yes"))
            {
                problemes.Add("Connexion root SSH autorisée");
                pointsPerdus += 8;
            }

            // Vérifier PasswordAuthentication
            if (config.Contains("PasswordAuthentication yes") && !config.Contains("#PasswordAuthentication yes"))
            {
                problemes.Add("Authentification par mot de passe activée");
                pointsPerdus += 6;
            }

            // Vérifier Port par défaut
            if (!config.Contains("Port ") || config.Contains("Port 22"))
            {
                problemes.Add("Port SSH par défaut (22) utilisé");
                pointsPerdus += 3;
            }

            if (problemes.Any())
            {
                verif.Reussie = false;
                verif.Niveau = pointsPerdus >= 10 ? NiveauRisque.Eleve : NiveauRisque.Moyen;
                verif.Points = 20 - pointsPerdus;
                verif.Details = string.Join(", ", problemes);
                verif.Recommandation = "Sécurisez SSH : désactivez root login, utilisez des clés SSH";
            }
            else
            {
                verif.Reussie = true;
                verif.Niveau = NiveauRisque.Aucun;
                verif.Points = 20;
                verif.Details = "Configuration SSH sécurisée";
                verif.Recommandation = "✅ SSH correctement configuré";
            }
        }
        catch (Exception ex)
        {
            verif.Reussie = false;
            verif.Niveau = NiveauRisque.Faible;
            verif.Points = 15;
            verif.Details = $"Vérification impossible : {ex.Message}";
            verif.Recommandation = "Vérifiez manuellement votre configuration SSH";
        }

        return verif;
    }

    /// <summary>
    /// Vérifier les mises à jour de sécurité
    /// </summary>
    public async Task<VerificationSecurite> VerifierMisesAJourSecuriteAsync()
    {
        var verif = new VerificationSecurite
        {
            Nom = "Mises à jour de sécurité",
            Description = "Vérification des correctifs de sécurité disponibles",
            PointsMax = 20
        };

        try
        {
            // Vérifier les updates disponibles (DNF/APT)
            var result = await _serviceCommandes.ExecuterCommandeAsync(
                "dnf check-update 2>/dev/null | tail -n +2 | wc -l || apt list --upgradable 2>/dev/null | wc -l"
            );
            
            if (int.TryParse(result.Output.Trim(), out int nbUpdates))
            {
                if (nbUpdates == 0)
                {
                    verif.Reussie = true;
                    verif.Niveau = NiveauRisque.Aucun;
                    verif.Points = 20;
                    verif.Details = "Système à jour";
                    verif.Recommandation = "✅ Aucune mise à jour de sécurité en attente";
                }
                else if (nbUpdates <= 5)
                {
                    verif.Reussie = false;
                    verif.Niveau = NiveauRisque.Faible;
                    verif.Points = 15;
                    verif.Details = $"{nbUpdates} mise(s) à jour de sécurité disponible(s)";
                    verif.Recommandation = "Installez les mises à jour de sécurité";
                    verif.CommandeCorrection = "sudo dnf update -y || sudo apt upgrade -y";
                }
                else
                {
                    verif.Reussie = false;
                    verif.Niveau = NiveauRisque.Moyen;
                    verif.Points = 10;
                    verif.Details = $"{nbUpdates} mises à jour de sécurité en attente";
                    verif.Recommandation = "⚠️ Installez rapidement les mises à jour";
                    verif.CommandeCorrection = "sudo dnf update -y || sudo apt upgrade -y";
                }
            }
        }
        catch (Exception ex)
        {
            verif.Reussie = false;
            verif.Niveau = NiveauRisque.Faible;
            verif.Points = 15;
            verif.Details = $"Vérification impossible : {ex.Message}";
            verif.Recommandation = "Vérifiez manuellement les mises à jour";
        }

        return verif;
    }

    /// <summary>
    /// Vérifier les ports ouverts
    /// </summary>
    public async Task<VerificationSecurite> VerifierPortsOuvertsAsync()
    {
        var verif = new VerificationSecurite
        {
            Nom = "Ports ouverts",
            Description = "Vérification des ports réseau exposés",
            PointsMax = 20
        };

        try
        {
            var result = await _serviceCommandes.ExecuterCommandeAsync("ss -tuln | grep LISTEN | wc -l");

            if (int.TryParse(result.Output.Trim(), out int nbPorts))
            {
                if (nbPorts <= 5)
                {
                    verif.Reussie = true;
                    verif.Niveau = NiveauRisque.Aucun;
                    verif.Points = 20;
                    verif.Details = $"{nbPorts} port(s) en écoute";
                    verif.Recommandation = "✅ Nombre de ports exposés acceptable";
                }
                else if (nbPorts <= 15)
                {
                    verif.Reussie = true;
                    verif.Niveau = NiveauRisque.Faible;
                    verif.Points = 15;
                    verif.Details = $"{nbPorts} ports en écoute";
                    verif.Recommandation = "Vérifiez que tous les services sont nécessaires";
                }
                else
                {
                    verif.Reussie = false;
                    verif.Niveau = NiveauRisque.Moyen;
                    verif.Points = 10;
                    verif.Details = $"{nbPorts} ports en écoute (beaucoup)";
                    verif.Recommandation = "⚠️ Désactivez les services inutiles";
                }
            }
        }
        catch (Exception ex)
        {
            verif.Reussie = false;
            verif.Niveau = NiveauRisque.Faible;
            verif.Points = 15;
            verif.Details = $"Vérification impossible : {ex.Message}";
            verif.Recommandation = "Vérifiez manuellement avec 'ss -tuln'";
        }

        return verif;
    }

    /// <summary>
    /// Vérifier les permissions des fichiers sensibles
    /// </summary>
    public async Task<VerificationSecurite> VerifierPermissionsFichiersAsync()
    {
        var verif = new VerificationSecurite
        {
            Nom = "Permissions fichiers",
            Description = "Vérification des permissions des fichiers critiques",
            PointsMax = 20
        };

        try
        {
            var problemes = new List<string>();

            // Vérifier /etc/passwd
            var resultPasswd = await _serviceCommandes.ExecuterCommandeAsync("stat -c '%a' /etc/passwd");
            if (resultPasswd.Output.Trim() != "644")
            {
                problemes.Add("/etc/passwd a des permissions incorrectes");
            }

            // Vérifier /etc/shadow
            var resultShadow = await _serviceCommandes.ExecuterCommandeAsync("stat -c '%a' /etc/shadow 2>/dev/null || echo '000'");
            var shadowPerms = resultShadow.Output.Trim();
            if (shadowPerms != "000" && shadowPerms != "640" && shadowPerms != "400")
            {
                problemes.Add("/etc/shadow a des permissions trop permissives");
            }

            if (problemes.Any())
            {
                verif.Reussie = false;
                verif.Niveau = NiveauRisque.Moyen;
                verif.Points = 12;
                verif.Details = string.Join(", ", problemes);
                verif.Recommandation = "Corrigez les permissions des fichiers système";
            }
            else
            {
                verif.Reussie = true;
                verif.Niveau = NiveauRisque.Aucun;
                verif.Points = 20;
                verif.Details = "Permissions correctes";
                verif.Recommandation = "✅ Fichiers système correctement protégés";
            }
        }
        catch (Exception ex)
        {
            verif.Reussie = false;
            verif.Niveau = NiveauRisque.Faible;
            verif.Points = 15;
            verif.Details = $"Vérification impossible : {ex.Message}";
            verif.Recommandation = "Vérifiez manuellement les permissions";
        }

        return verif;
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