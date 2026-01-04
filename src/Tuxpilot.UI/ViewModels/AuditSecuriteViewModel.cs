using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
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
    private ObservableCollection<VerificationSecurite> _verificationsFiltrees = new();

    [ObservableProperty]
    private bool _afficherUniquementProblemes;

    [ObservableProperty]
    private bool _afficherUniquementCritiques;

    [ObservableProperty]
    private string? _categorieSelectionnee;

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

    partial void OnAfficherUniquementProblemesChanged(bool value)
    => AppliquerFiltres();

    partial void OnAfficherUniquementCritiquesChanged(bool value)
        => AppliquerFiltres();

    partial void OnCategorieSelectionneeChanged(string? value)
        => AppliquerFiltres();

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
                Verifications.Add(verif);

            AppliquerFiltres();

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

    private void AppliquerFiltres()
    {
        VerificationsFiltrees.Clear();

        if (Verifications == null)
            return;

        IEnumerable<VerificationSecurite> query = Verifications;

        if (AfficherUniquementProblemes)
            query = query.Where(v => !v.Reussie);

        if (AfficherUniquementCritiques)
            query = query.Where(v =>
                v.Niveau == NiveauRisque.Eleve ||
                v.Niveau == NiveauRisque.Critique);

        if (!string.IsNullOrWhiteSpace(CategorieSelectionnee))
            query = query.Where(v => v.Categorie == CategorieSelectionnee);

        foreach (var v in query)
            VerificationsFiltrees.Add(v);
    }

}