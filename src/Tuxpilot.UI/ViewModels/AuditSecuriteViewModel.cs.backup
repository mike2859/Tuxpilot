using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.UI.ViewModels;


/// <summary>
/// ViewModel pour l'audit de sécurité
/// </summary>
public partial class AuditSecuriteViewModel : ViewModelBase
{
    private readonly IServiceSecurite _serviceSecurite;

    [ObservableProperty]
    private RapportSecurite? _rapport;

    [ObservableProperty]
    private ObservableCollection<VerificationSecurite> _verifications = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _auditEffectue;

    public AuditSecuriteViewModel(IServiceSecurite serviceSecurite)
    {
        _serviceSecurite = serviceSecurite;
    }

    /// <summary>
    /// Lancer l'audit de sécurité
    /// </summary>
    [RelayCommand]
    private async Task LancerAuditAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Analyse de sécurité en cours...";
            AuditEffectue = false;

            Rapport = await _serviceSecurite.ExecuterAuditAsync();

            Verifications.Clear();
            foreach (var verif in Rapport.Verifications)
            {
                Verifications.Add(verif);
            }

            AuditEffectue = true;
            StatusMessage = $"✅ Audit terminé - Score : {Rapport.Score}/100";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Appliquer une correction
    /// </summary>
    [RelayCommand]
    private async Task AppliquerCorrectionAsync(string? commande)
    {
        if (string.IsNullOrEmpty(commande))
            return;

        try
        {
            StatusMessage = "Application de la correction...";
            var success = await _serviceSecurite.AppliquerCorrectionAsync(commande);

            if (success)
            {
                StatusMessage = "✅ Correction appliquée avec succès";
                // Relancer l'audit
                await Task.Delay(1000);
                await LancerAuditAsync();
            }
            else
            {
                StatusMessage = "❌ Échec de la correction";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Erreur : {ex.Message}";
        }
    }
}