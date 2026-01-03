using System.Diagnostics;
using System.Text;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Service d'exécution de commandes système
/// </summary>
public class ServiceCommandes : IServiceCommandes
{
    public async Task<(bool Success, string Output)> ExecuterCommandeAsync(string command, bool needsSudo = true)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = needsSudo ? "pkexec" : "/bin/bash",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (needsSudo)
            {
                processInfo.Arguments = $"bash -c \"{command}\"";
            }
            else
            {
                processInfo.Arguments = $"-c \"{command}\"";
            }

            using var process = new Process { StartInfo = processInfo };
            
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    error.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var success = process.ExitCode == 0;
            var result = success ? output.ToString() : error.ToString();

            return (success, result);
        }
        catch (Exception ex)
        {
            return (false, $"Erreur: {ex.Message}");
        }
    }

    public async Task ExecuterCommandeAvecLogsAsync(string command, Action<string> onLogReceived, bool needsSudo = true)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = needsSudo ? "pkexec" : "/bin/bash",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (needsSudo)
            {
                processInfo.Arguments = $"bash -c \"{command}\"";
            }
            else
            {
                processInfo.Arguments = $"-c \"{command}\"";
            }

            using var process = new Process { StartInfo = processInfo };

            // 🆕 Appeler directement le callback (le ViewModel gérera le dispatch)
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    onLogReceived(e.Data + "\n");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    onLogReceived($"[ERROR] {e.Data}\n");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var message = process.ExitCode == 0 
                ? "\n✅ Commande exécutée avec succès !" 
                : $"\n❌ Erreur (code {process.ExitCode})";

            onLogReceived(message);
        }
        catch (Exception ex)
        {
            onLogReceived($"\n❌ Erreur : {ex.Message}");
        }
    }
}