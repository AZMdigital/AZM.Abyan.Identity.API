namespace AZM.Abyan.Identity.Application.Services;

public interface ISyncOrchestratorService
{
    Task<SyncResult> SyncAllAsync(CancellationToken cancellationToken = default);
}

public class SyncResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, SyncEntityResult> EntityResults { get; set; } = new();
}

public class SyncEntityResult
{
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Deleted { get; set; }
    public List<string> Errors { get; set; } = new();
}

