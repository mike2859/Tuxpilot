using Tuxpilot.Core.Entities;
using Tuxpilot.Infrastructure.Dtos;

namespace Tuxpilot.Infrastructure.Extensions;


/// <summary>
/// Extensions pour mapper les DTOs de mise à jour
/// </summary>
public static class UpdateInfoExtensions
{
    /// <summary>
    /// Convertit un DTO UpdateInfo en entité
    /// </summary>
    public static UpdateInfo ToEntity(this UpdateInfoDto dto)
    {
        return new UpdateInfo
        {
            Gestionnaire = dto.Gestionnaire,
            Nombre = dto.Nombre,
            Paquets = dto.Paquets.Select(p => p.ToEntity()).ToList(),
            Erreur = dto.Erreur
        };
    }
    
    /// <summary>
    /// Convertit un DTO Package en entité
    /// </summary>
    public static Package ToEntity(this PackageDto dto)
    {
        return new Package
        {
            Nom = dto.Nom,
            VersionActuelle = dto.VersionActuelle,
            VersionDisponible = dto.VersionDisponible,
            Depot = dto.Depot
        };
    }
}