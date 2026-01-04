using System.Threading.Tasks;
using Tuxpilot.Core.Entities;

namespace Tuxpilot.Core.Interfaces.Services;

public interface IServiceContexteSysteme
{
    Task<SystemContextSnapshot> GetSnapshotAsync(bool forceRefresh = false);
}
