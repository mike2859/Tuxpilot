using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour afficher un paquet
/// </summary>
public partial class PackageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nom = string.Empty;
    
    [ObservableProperty]
    private string _versionActuelle = string.Empty;
    
    [ObservableProperty]
    private string _versionDisponible = string.Empty;
    
    [ObservableProperty]
    private string _depot = string.Empty;
    
    /// <summary>
    /// Affichage formaté des versions
    /// </summary>
    public string VersionText => $"{VersionActuelle} → {VersionDisponible}";
}