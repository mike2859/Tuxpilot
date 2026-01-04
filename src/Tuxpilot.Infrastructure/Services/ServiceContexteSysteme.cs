using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;

public class ServiceContexteSysteme : IServiceContexteSysteme
{
    private readonly IServiceSecurite _serviceSecurite;

    // Cache (évite de relancer un audit/commandes à chaque message)
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SystemContextSnapshot? _cache;
    private DateTimeOffset _cacheAt;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(20);

    public ServiceContexteSysteme(IServiceSecurite serviceSecurite)
    {
        _serviceSecurite = serviceSecurite;
    }

    public async Task<SystemContextSnapshot> GetSnapshotAsync(bool forceRefresh = false)
    {
        await _lock.WaitAsync();
        try
        {
            if (!forceRefresh && _cache != null && (DateTimeOffset.UtcNow - _cacheAt) < _ttl)
                return _cache;

            var snap = new SystemContextSnapshot
            {
                CapturedAt = DateTimeOffset.UtcNow
            };

            // Système
            snap.OsPrettyName = await ReadOsPrettyNameAsync();
            snap.Kernel = (await RunAsync("uname -r")).Trim();
            snap.Hostname = (await RunAsync("hostname")).Trim();
            snap.Desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "";

            // Réseau / firewall (rapide)
            snap.FirewallStatus = await DetectFirewallAsync();
            snap.ExposedPorts = await GetExposedPortsAsync();

            // Reboot required
            snap.RebootRequired = await DetectRebootRequiredAsync();

            // Audit sécurité (ta source de vérité)
            var rapport = await _serviceSecurite.ExecuterAuditAsync(); // réutilise ton pipeline 
            snap.SecurityScore = rapport.Score;

            // Top issues : prendre les plus graves + avec preuves
            var ordered = rapport.Verifications
                .OrderByDescending(v => v.Niveau == NiveauRisque.Critique)
                .ThenByDescending(v => v.Niveau == NiveauRisque.Eleve)
                .ThenByDescending(v => v.Niveau == NiveauRisque.Moyen)
                .ThenByDescending(v => v.Niveau == NiveauRisque.Faible)
                .ThenBy(v => v.Nom)
                .ToList();

            snap.TopIssues = ordered
                .Where(v => !v.Reussie || v.Niveau != NiveauRisque.Aucun)
                .Take(8)
                .Select(v => new SecurityIssue
                {
                    Id = v.Id ?? "",
                    Name = v.Nom,
                    Level = v.Niveau.ToString(),
                    Details = v.Details,
                    Proof = v.Preuve ?? "",
                    Impact = v.Impact ?? "",
                    Recommendation = v.Recommandation
                })
                .ToList();

            _cache = snap;
            _cacheAt = DateTimeOffset.UtcNow;
            return snap;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static async Task<string> ReadOsPrettyNameAsync()
    {
        try
        {
            var txt = await System.IO.File.ReadAllTextAsync("/etc/os-release");
            foreach (var line in txt.Split('\n'))
            {
                if (line.StartsWith("PRETTY_NAME="))
                    return line.Split('=', 2)[1].Trim().Trim('"');
            }
        }
        catch { }
        return "";
    }

    private static async Task<string> DetectFirewallAsync()
    {
        var ufw = (await RunAsync("systemctl is-active ufw 2>/dev/null || true")).Trim();
        if (ufw == "active") return "ufw active";

        var fw = (await RunAsync("systemctl is-active firewalld 2>/dev/null || true")).Trim();
        if (fw == "active") return "firewalld active";

        return "none";
    }

    private static async Task<List<string>> GetExposedPortsAsync()
    {
        // Exposés = écoute sur 0.0.0.0 / :: / IP non loopback
        var output = await RunAsync("ss -H -tulnp 2>/dev/null || true");
        var res = new List<string>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) continue;

            var local = parts[4]; // addr:port
            if (!local.Contains(':')) continue;

            var addr = local[..local.LastIndexOf(':')];
            var port = local[(local.LastIndexOf(':') + 1)..];

            bool isLocal = addr.Contains("127.0.0.1") || addr.Contains("::1");
            if (isLocal) continue;

            // détecter processus (optionnel)
            var users = parts.Length >= 7 ? string.Join(' ', parts.Skip(6)) : "";
            res.Add($"{port} {users}".Trim());
        }

        return res.Distinct().Take(12).ToList();
    }

    private static async Task<bool?> DetectRebootRequiredAsync()
    {
        // Debian/Ubuntu
        if (System.IO.File.Exists("/var/run/reboot-required"))
            return true;

        // Fedora : needs-restarting (si dispo)
        var which = await RunAsync("command -v needs-restarting >/dev/null 2>&1; echo $?");
        if (which.Trim() == "0")
        {
            var r = await RunAsync("needs-restarting -r >/dev/null 2>&1; echo $?");
            // needs-restarting -r => 1 quand reboot nécessaire
            if (r.Trim() == "1") return true;
            if (r.Trim() == "0") return false;
        }
        return null; // inconnu
    }

    private static async Task<string> RunAsync(string cmd)
    {
        var psi = new ProcessStartInfo("/bin/bash", $"-lc \"{cmd}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var p = Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();
        return stdout;
    }
}
