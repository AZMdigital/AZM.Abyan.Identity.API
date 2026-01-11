using System.Reflection;

namespace AZM.Abyan.Identity.Application.Services;

public interface IPermissionSyncService
{
    Task SyncPermissionsAsync(Assembly assembly, CancellationToken cancellationToken = default);
}

