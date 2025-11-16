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
            throw new Exception($"Erreur script: {erreur}");
        
        return sortie;
    }
}