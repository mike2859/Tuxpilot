using Tuxpilot.Core.Entities;
using Tuxpilot.Infrastructure.Dtos;

namespace Tuxpilot.Infrastructure.Extensions;

/// <summary>
/// Extensions pour mapper les DTOs d'informations système
/// </summary>
public static class SystemInfoExtensions
{
    /// <summary>
    /// Convertit un DTO SystemInfo en entité
    /// </summary>
    public static SystemInfo ToEntity(this SystemInfoDto dto)
    {
        return new SystemInfo
        {
            Distribution = dto.Distribution,
            VersionKernel = dto.Kernel,
            CpuModel = dto.CpuModel,
            CpuCores = dto.CpuCores,
            CpuThreads = dto.CpuThreads,
            RamTotaleMB = dto.RamTotaleMB,
            RamUtiliseeMB = dto.RamUtiliseeMB,
            RamLibreMB = dto.RamLibreMB,
            PourcentageRam = dto.PourcentageRam,
            PourcentageCpu = dto.PourcentageCpu,
            PourcentageDisque = dto.PourcentageDisque,
            GestionnairePaquets = dto.GestionnairePaquets
        };
    }
}