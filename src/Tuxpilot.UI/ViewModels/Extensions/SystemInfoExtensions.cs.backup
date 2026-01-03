using Tuxpilot.Core.Entities;

namespace Tuxpilot.UI.ViewModels.Extensions;

public static class SystemInfoExtensions
{
    /// <summary>
    /// Convertit une entité SystemInfo en ViewModel
    /// </summary>
    public static SystemInfoViewModel ToViewModel(this SystemInfo entity)
    {
        return new SystemInfoViewModel
        {
            Distribution = entity.Distribution,
            KernelVersion = entity.VersionKernel,
            CpuModel = entity.CpuModel,
            CpuCores = entity.CpuCores,
            CpuThreads = entity.CpuThreads,
            TotalRamMB = entity.RamTotaleMB,
            UsedRamMB = entity.RamUtiliseeMB,
            RamPercent = entity.PourcentageRam,
            CpuPercent = entity.PourcentageCpu,
            DiskPercent = entity.PourcentageDisque,
            PackageManager = entity.GestionnairePaquets
        };
    }
}