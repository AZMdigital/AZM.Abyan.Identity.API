using AZM.Abyan.Identity.Domain.Entities;

namespace AZM.Abyan.Identity.Domain.Interfaces;

public interface ILicenseRepository
{
    Task<License?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task           AddAsync(License license, CancellationToken ct = default);
    void           Update(License license);
    Task           SaveChangesAsync(CancellationToken ct = default);
}
