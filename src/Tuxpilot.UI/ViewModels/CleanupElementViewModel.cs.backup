using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour afficher un élément nettoyable
/// </summary>
public partial class CleanupElementViewModel : ObservableObject
{
    [ObservableProperty]
    private string _type = string.Empty;
    
    [ObservableProperty]
    private string _nom = string.Empty;
    
    [ObservableProperty]
    private string _chemin = string.Empty;
    
    [ObservableProperty]
    private long _tailleMB;
    
    [ObservableProperty]
    private int _nombreFichiers;
    
    [ObservableProperty]
    private int _nombrePaquets;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    /// <summary>
    /// Texte formaté de la taille
    /// </summary>
    public string TailleTexte
    {
        get
        {
            if (TailleMB < 1024)
                return $"{TailleMB} MB";
            else
                return $"{TailleMB / 1024.0:F1} GB";
        }
    }
    
    /// <summary>
    /// Icône selon le type
    /// </summary>
    public string Icone => Type switch
    {
        "cache_paquets" => "📦",
        "logs_anciens" => "📋",
        "paquets_orphelins" => "🗑️",
        "fichiers_temporaires" => "🗂️",
        _ => "📄"
    };
}