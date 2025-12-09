using System.Text.Json;
using Tuxpilot.Core.Entities;
using Tuxpilot.Core.Enums;
using Tuxpilot.Core.Interfaces.Services;
using Tuxpilot.Infrastructure.Dtos;

namespace Tuxpilot.Infrastructure.Services;

/// <summary>
/// Implémentation du service de gestion des licences avec validation API
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly ExecuteurScriptPython _executeur;
    private License? _cachedLicense;

    public LicenseService(ExecuteurScriptPython executeur)
    {
        _executeur = executeur;
    }

    public async Task<License> GetCurrentLicenseAsync()
    {
        // Utiliser le cache si disponible
        if (_cachedLicense != null)
            return _cachedLicense;

        try
        {
            // Exécuter le script Python avec commande "check" (offline)
            var output = await _executeur.ExecuterAsync("validate_license.py", "check");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var licenseData = JsonSerializer.Deserialize<LicenseApiResponse>(output, options);

            if (licenseData == null)
            {
                return GetCommunityLicense("Erreur de désérialisation");
            }

            var license = MapToLicense(licenseData);

            // Mettre en cache
            _cachedLicense = license;

            return license;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LICENSE ERROR] {ex.Message}");
            return GetCommunityLicense($"Erreur: {ex.Message}");
        }
    }

    public async Task<License> ActivateLicenseAsync(string licenseKey)
    {
        try
        {
            // Exécuter le script avec commande "activate" (appelle l'API)
            var output = await _executeur.ExecuterAsync("validate_license.py", $"activate \"{licenseKey}\"");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var licenseData = JsonSerializer.Deserialize<LicenseApiResponse>(output, options);

            if (licenseData == null)
            {
                return GetInvalidLicense("Erreur de désérialisation");
            }

            var license = MapToLicense(licenseData);

            // Mettre à jour le cache si valide
            if (license.IsValid)
            {
                _cachedLicense = license;
            }

            return license;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LICENSE ACTIVATION ERROR] {ex.Message}");
            return GetInvalidLicense($"Erreur: {ex.Message}");
        }
    }

    public async Task<bool> RevokeLicenseAsync()
    {
        try
        {
            await _executeur.ExecuterAsync("validate_license.py", "revoke");

            // Invalider le cache
            _cachedLicense = null;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LICENSE REVOKE ERROR] {ex.Message}");
            return false;
        }
    }

    public async Task<bool> HasFeatureAsync(string feature)
    {
        var license = await GetCurrentLicenseAsync();
        return license.HasFeature(feature);
    }

    public async Task<List<string>> GetAvailableFeaturesAsync()
    {
        var license = await GetCurrentLicenseAsync();
        return license.Features;
    }

    /// <summary>
    /// Mappe le résultat JSON vers l'entité License
    /// </summary>
    private License MapToLicense(LicenseApiResponse data)
    {
        // Mapper le type de licence
        var licenseType = data.Type?.ToLower() switch
        {
            "community" => LicenseType.Community,
            "pro" => LicenseType.Pro,
            "organization" => LicenseType.Organization,
            _ => LicenseType.Community
        };

        // Parser dates si présentes
        DateTime? activatedAt = null;
        if (!string.IsNullOrEmpty(data.ActivatedAt))
        {
            if (DateTime.TryParse(data.ActivatedAt, out var parsed))
                activatedAt = parsed;
        }

        DateTime? expiresAt = null;
        if (!string.IsNullOrEmpty(data.ExpiresAt))
        {
            if (DateTime.TryParse(data.ExpiresAt, out var parsed))
                expiresAt = parsed;
        }

        return new License
        {
            Key = data.Key ?? string.Empty,
            Type = licenseType,
            IsValid = data.Valid,
            Features = data.Features ?? new List<string>(),
            Error = data.Error,
            IsStored = data.Stored,
            ActivatedAt = activatedAt,
            ExpiresAt = expiresAt,
            UserId = data.UserId,
            Metadata = data.Metadata
        };
    }

    /// <summary>
    /// Retourne une licence Community par défaut
    /// </summary>
    private License GetCommunityLicense(string? error = null)
    {
        return new License
        {
            Key = string.Empty,
            Type = LicenseType.Community,
            IsValid = true,
            Features = new List<string> { "dashboard", "monitoring", "updates_manual" },
            Error = error,
            IsStored = false
        };
    }

    /// <summary>
    /// Retourne une licence invalide
    /// </summary>
    private License GetInvalidLicense(string error)
    {
        return new License
        {
            Key = string.Empty,
            Type = LicenseType.Community,
            IsValid = false,
            Error = error,
            Features = new List<string>()
        };
    }

}