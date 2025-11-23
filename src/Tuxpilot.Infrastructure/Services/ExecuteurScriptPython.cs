using System.Diagnostics;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Exécute les scripts Python du projet
/// </summary>
public class ExecuteurScriptPython
{
    private readonly string _cheminScripts;
    
    public ExecuteurScriptPython()
    {
        // Chemin vers les scripts Python
        _cheminScripts = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Scripts"
        );
    }
    
    /// <summary>
    /// Exécute un script Python et retourne sa sortie
    /// </summary>
    public async Task<string> ExecuterAsync(string nomScript, string arguments = "")
    {
        var cheminScript = Path.Combine(_cheminScripts, nomScript);
        
        if (!File.Exists(cheminScript))
            throw new FileNotFoundException($"Script introuvable: {cheminScript}");
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"\"{cheminScript}\" {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = startInfo };
        process.Start();
        
        var sortie = await process.StandardOutput.ReadToEndAsync();
        var erreur = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            Console.WriteLine($"[SCRIPT ERROR] Code sortie: {process.ExitCode}");
            Console.WriteLine($"[SCRIPT ERROR] Stdout: {sortie}");
            Console.WriteLine($"[SCRIPT ERROR] Stderr: {erreur}");
            throw new Exception($"Erreur script (code {process.ExitCode}): {erreur}\nStdout: {sortie}");
        }
        
        return sortie;
    }
    
    /// <summary>
    /// Exécute un script Python avec streaming ligne par ligne
    /// </summary>
    public async Task ExecuterAvecStreamingAsync(
        string nomScript, 
        Action<string> onLineReceived, 
        string arguments = "")
    {
        var cheminScript = Path.Combine(_cheminScripts, nomScript);
        
        if (!File.Exists(cheminScript))
            throw new FileNotFoundException($"Script introuvable: {cheminScript}");
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"\"{cheminScript}\" {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = startInfo };
        
        // Handler pour lire les lignes en temps réel
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                onLineReceived(e.Data);
            }
        };
        
        process.Start();
        process.BeginOutputReadLine();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            var erreur = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Erreur script: {erreur}");
        }
    }
}