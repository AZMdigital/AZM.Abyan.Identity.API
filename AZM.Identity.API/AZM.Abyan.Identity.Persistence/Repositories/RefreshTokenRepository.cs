using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Persistence.Repositories;

public class RefreshTokenRepository(IdentityDbContext context) : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked, ct);
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Id == id, ct);
    }

    public async Task<List<RefreshToken>> GetByLicenseIdAsync(Guid licenseId, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.LicenseId == licenseId && !rt.IsRevoked)
            .ToListAsync(ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }

    public void Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    public void Delete(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
    }
}