using System.Text.Json;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Service de gestion du thème
/// </summary>
public class ServiceTheme : IServiceTheme
{
    private readonly string _configFile;
    private Theme _themeActuel = Theme.Light;
    
    public Theme ThemeActuel => _themeActuel;
    
    public ServiceTheme()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".tuxpilot"
        );
        
        Directory.CreateDirectory(appDataPath);
        _configFile = Path.Combine(appDataPath, "theme.json");
        
        // Charger le thème au démarrage
        _themeActuel = ChargerTheme();
    }
    
    public void ChangerTheme(Theme theme)
    {
        _themeActuel = theme;
        SauvegarderTheme(theme);
    }
    
    public Theme ChargerTheme()
    {
        try
        {
            if (!File.Exists(_configFile))
                return Theme.Light;
            
            var json = File.ReadAllText(_configFile);
            var config = JsonSerializer.Deserialize<ThemeConfig>(json);
            
            return config?.Theme ?? Theme.Light;
        }
        catch
        {
            return Theme.Light;
        }
    }
    
    public void SauvegarderTheme(Theme theme)
    {
        try
        {
            var config = new ThemeConfig { Theme = theme };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(_configFile, json);
        }
        catch
        {
            // Ignorer les erreurs de sauvegarde
        }
    }
    
    private class ThemeConfig
    {
        public Theme Theme { get; set; }
    }
}