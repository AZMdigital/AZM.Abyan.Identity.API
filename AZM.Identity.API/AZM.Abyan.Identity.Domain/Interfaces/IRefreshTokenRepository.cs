using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<RefreshToken>> GetByLicenseIdAsync(Guid licenseId, CancellationToken ct);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct);
    void Update(RefreshToken refreshToken);
    void Delete(RefreshToken refreshToken);
}