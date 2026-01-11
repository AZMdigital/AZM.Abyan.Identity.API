using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class ClientSyncService : IClientSyncService
{
    private readonly IKeycloakService _keycloakService;
    private readonly IRepository<Client, Guid> _clientRepository;
    private readonly IdentityDbContext _dbContext;

    public ClientSyncService(
        IKeycloakService keycloakService,
        IRepository<Client, Guid> clientRepository,
        IdentityDbContext dbContext)
    {
        _keycloakService = keycloakService;
        _clientRepository = clientRepository;
        _dbContext = dbContext;
    }

    public async Task<SyncEntityResult> SyncClientsAsync(string realm, Guid tenantId, string adminToken, CancellationToken cancellationToken = default)
    {
        var result = new SyncEntityResult();

        try
        {
            // Get all clients from Keycloak for this realm
            var keycloakClients = await _keycloakService.GetClientsAsync(realm, adminToken, cancellationToken);

            // Get all clients from local database for this tenant
            var localClients = await _clientRepository.GetWhere(c => c.RealmId == tenantId).ToListAsync(cancellationToken);

            // Create a dictionary of Keycloak clients by ID
            var keycloakClientsDict = keycloakClients.ToDictionary(c => c.Id, c => c);

            // Process each Keycloak client
            foreach (var keycloakClient in keycloakClients)
            {
                var localClient = localClients.FirstOrDefault(c => c.KeycloakClientId?.ToString() == keycloakClient.Id);

                if (localClient == null)
                {
                    // Create new client
                    localClient = new Client
                    {
                        Id = Guid.NewGuid(),
                        Name = keycloakClient.ClientId, // Use ClientId instead of Name (ClientId is the actual identifier, Name is optional)
                        Description = keycloakClient.Description ?? string.Empty,
                        KeycloakClientId = Guid.TryParse(keycloakClient.Id, out var clientId) ? clientId : null,
                        RealmId = tenantId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    await _clientRepository.CreateAsync(localClient, cancellationToken);
                    result.Added++;
                }
                else
                {
                    // Update existing client
                    localClient.Name = keycloakClient.ClientId; // Use ClientId instead of Name (ClientId is the actual identifier, Name is optional)
                    localClient.Description = keycloakClient.Description ?? string.Empty;
                    if (Guid.TryParse(keycloakClient.Id, out var clientId))
                    {
                        localClient.KeycloakClientId = clientId;
                    }
                    localClient.UpdatedAt = DateTime.UtcNow;
                    localClient.UpdatedBy = Guid.Empty;
                    _clientRepository.Update(localClient);
                    result.Updated++;
                }
            }

            // Delete clients that don't exist in Keycloak
            var keycloakClientIds = keycloakClients.Select(c => c.Id).ToHashSet();
            var clientsToDelete = localClients
                .Where(c => c.KeycloakClientId.HasValue && !keycloakClientIds.Contains(c.KeycloakClientId.Value.ToString()))
                .ToList();

            foreach (var clientToDelete in clientsToDelete)
            {
                _dbContext.Clients.Remove(clientToDelete);
                result.Deleted++;
            }

            await _clientRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error syncing clients: {ex.Message}");
        }

        return result;
    }
}

