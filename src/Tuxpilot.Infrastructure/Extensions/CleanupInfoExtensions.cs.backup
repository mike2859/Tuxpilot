using Tuxpilot.Core.Entities;
using Tuxpilot.Infrastructure.Dtos;

namespace Tuxpilot.Infrastructure.Extensions;

/// <summary>
/// Extensions pour mapper les DTOs de nettoyage
/// </summary>
public static class CleanupInfoExtensions
{
    /// <summary>
    /// Convertit un DTO CleanupInfo en entité
    /// </summary>
    public static CleanupInfo ToEntity(this CleanupInfoDto dto)
    {
        return new CleanupInfo
        {
            Gestionnaire = dto.Gestionnaire,
            Elements = dto.Elements.Select(e => e.ToEntity()).ToList(),
            TailleTotaleMB = dto.TailleTotaleMB,
            NombreElements = dto.NombreElements,
            Erreur = dto.Erreur
        };
    }
    
    /// <summary>
    /// Convertit un DTO CleanupElement en entité
    /// </summary>
    public static CleanupElement ToEntity(this CleanupElementDto dto)
    {
        return new CleanupElement
        {
            Type = dto.Type,
            Nom = dto.Nom,
            Chemin = dto.Chemin,
            TailleMB = dto.TailleMB,
            NombreFichiers = dto.NombreFichiers,
            NombrePaquets = dto.NombrePaquets,
            Description = dto.Description
        };
    }
}