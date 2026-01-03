using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour un message de log d'installation
/// </summary>
public partial class InstallLogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _message = string.Empty;
    
    [ObservableProperty]
    private string _icon = "ℹ️";
    
    [ObservableProperty]
    private string _color = "TextSecondary";
    
    public InstallLogViewModel(string type, string message)
    {
        Message = message;
        
        // Définir l'icône et la couleur selon le type
        (Icon, Color) = type switch
        {
            "info" => ("ℹ️", "TextSecondary"),
            "download" => ("⬇️", "Info"),
            "install" => ("📦", "#8B5CF6"),
            "setup" => ("⚙️", "#6366F1"),
            "success" => ("✅", "Success"),
            "final_success" => ("🎉", "Success"),
            "error" => ("❌", "Danger"),
            "warning" => ("⚠️", "Warning"),
            _ => ("•", "TextMuted")
        };
    }
}