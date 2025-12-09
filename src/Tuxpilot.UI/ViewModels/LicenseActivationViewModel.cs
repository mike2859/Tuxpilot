using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;

/// <summary>
/// ViewModel pour l'activation de licence
/// </summary>
public partial class LicenseActivationViewModel : ViewModelBase
{
    private readonly ILicenseService _licenseService;
    
    [ObservableProperty]
    private string _licenseKey = string.Empty;
    
    [ObservableProperty]
    private bool _isActivating;
    
    [ObservableProperty]
    private bool _isSuccess;
    
    [ObservableProperty]
    private bool _isError;
    
    [ObservableProperty]
    private string _message = string.Empty;
    
    [ObservableProperty]
    private License? _activatedLicense;
    
    public LicenseActivationViewModel(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }
    
    /// <summary>
    /// Active la licence avec la clé fournie (appelle l'API)
    /// </summary>
    [RelayCommand]
    private async Task ActivateLicenseAsync()
    {
        // Reset états
        IsSuccess = false;
        IsError = false;
        Message = string.Empty;
        
        // Validation basique
        if (string.IsNullOrWhiteSpace(LicenseKey))
        {
            IsError = true;
            Message = "Veuillez entrer une clé de licence";
            return;
        }
        
        IsActivating = true;
        
        try
        {
            // Activer la licence (appel API via script Python)
            var license = await _licenseService.ActivateLicenseAsync(LicenseKey.Trim());
            
            if (license.IsValid)
            {
                // Succès
                IsSuccess = true;
              //  ActivatedLicense = license;
                
                // Message selon le type
                if (license.IsEarlyAccess)
                {
                    Message = "✅ Licence Early Access activée avec succès !\n\n" +
                            "🎁 Vous bénéficiez de :\n" +
                            "• 1 an de version Pro gratuite (180€)\n" +
                            "• 50% de réduction à vie après (7,50€/mois au lieu de 15€)\n" +
                            "• Accès prioritaire aux nouvelles fonctionnalités";
                }
                else
                {
                    Message = $"✅ Licence {license.TypeDisplayName} activée avec succès !";
                }
            }
            else
            {
                // Erreur
                IsError = true;
                Message = $"❌ {license.Error ?? "Clé de licence invalide"}";
            }
        }
        catch (Exception ex)
        {
            IsError = true;
            Message = $"❌ Erreur lors de l'activation : {ex.Message}";
        }
        finally
        {
            IsActivating = false;
        }
    }
    
    /// <summary>
    /// Continue avec la version Community gratuite
    /// </summary>
    [RelayCommand]
    private async Task UseCommunityAsync()
    {
        try
        {
            // Vérifier la licence actuelle (Community par défaut)
            var license = await _licenseService.GetCurrentLicenseAsync();
            
            IsSuccess = true;
            //ActivatedLicense = license.IsValid;
            Message = "Mode Community activé !\n\n" +
                     "Fonctionnalités disponibles :\n" +
                     "• Dashboard\n" +
                     "• Monitoring système\n" +
                     "• Mises à jour manuelles";
        }
        catch (Exception ex)
        {
            IsError = true;
            Message = $"Erreur : {ex.Message}";
        }
    }
    
    /// <summary>
    /// Formate la clé pendant la saisie (ajoute les tirets automatiquement)
    /// Format cible : EA-2024-ABCD-1234
    /// </summary>
    partial void OnLicenseKeyChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        
        // Enlever tous les tirets
        var cleaned = value.Replace("-", "").ToUpper();
        
        // Limiter selon le format EA-2024-ABCD-1234
        // Partie 1: EA/PRO/ORG (2-3 chars)
        // Partie 2: 2024 (4 chars)
        // Partie 3: ABCD (4 chars)
        // Partie 4: 1234 (4 chars)
        // Total max: ~17 chars
        
        if (cleaned.Length > 17)
            cleaned = cleaned.Substring(0, 17);
        
        // Ajouter les tirets
        var formatted = string.Empty;
        
        // Partie 1: Type (2-3 chars)
        if (cleaned.Length > 0)
        {
            var typeLength = cleaned.Length >= 3 && 
                (cleaned.StartsWith("PRO") || cleaned.StartsWith("ORG")) ? 3 : 2;
            formatted += cleaned.Substring(0, Math.Min(typeLength, cleaned.Length));
            
            if (cleaned.Length > typeLength)
            {
                formatted += "-";
                
                // Partie 2: Année (4 chars)
                var yearStart = typeLength;
                var yearLength = Math.Min(4, cleaned.Length - yearStart);
                formatted += cleaned.Substring(yearStart, yearLength);
                
                if (cleaned.Length > yearStart + yearLength)
                {
                    formatted += "-";
                    
                    // Partie 3: Random (4 chars)
                    var part3Start = yearStart + yearLength;
                    var part3Length = Math.Min(4, cleaned.Length - part3Start);
                    formatted += cleaned.Substring(part3Start, part3Length);
                    
                    if (cleaned.Length > part3Start + part3Length)
                    {
                        formatted += "-";
                        
                        // Partie 4: Random (4 chars)
                        var part4Start = part3Start + part3Length;
                        formatted += cleaned.Substring(part4Start);
                    }
                }
            }
        }
        
        // Mettre à jour si différent (éviter boucle infinie)
        if (formatted != value)
        {
            LicenseKey = formatted;
        }
    }
}