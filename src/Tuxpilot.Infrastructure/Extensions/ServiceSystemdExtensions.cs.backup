using Tuxpilot.Core.Entities;
using Tuxpilot.Infrastructure.Dtos;

namespace Tuxpilot.Infrastructure.Extensions;

public static class ServiceSystemdExtensions
{
    public static ServiceSystemd ToEntity(this ServiceSystemdDto dto)
    {
        return new ServiceSystemd
        {
            Name = dto.Name,
            Status = dto.Status,
            Enabled = dto.Enabled,
            Unit = dto.Unit
        };
    }
}