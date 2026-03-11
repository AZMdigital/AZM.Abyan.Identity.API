using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Infrastructure.Services;

public class ClientSyncService(
    IKeycloakService keycloakService,
    IRepository<Client, Guid> clientRepository,
    IdentityDbContext dbContext) : IClientSyncService
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly IRepository<Client, Guid> _clientRepository = clientRepository;
    private readonly IdentityDbContext _dbContext = dbContext;

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
                if (!Guid.TryParse(keycloakClient.Id.ToString(), out var keycloakClientId))
                {
                    result.Errors.Add($"Invalid Keycloak client ID format: {keycloakClient.Id}");
                    continue;
                }

                var localClient = localClients.FirstOrDefault(c => c.Id == keycloakClientId);

                if (localClient == null)
                {
                    // Create new client
                    localClient = new Client
                    {
                        Id = keycloakClientId,
                        Name = keycloakClient.ClientId, // Use ClientId instead of Name (ClientId is the actual identifier, Name is optional)
                        Description = keycloakClient.Description ?? string.Empty,
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
                    localClient.UpdatedAt = DateTime.UtcNow;
                    localClient.UpdatedBy = Guid.Empty;
                    _clientRepository.Update(localClient);
                    result.Updated++;
                }
            }

            // Delete clients that don't exist in Keycloak
            var keycloakClientIds = keycloakClients
                .Where(c => Guid.TryParse(c.Id.ToString(), out _))
                .Select(c => Guid.Parse(c.Id.ToString()))
                .ToHashSet();
            var clientsToDelete = localClients
                .Where(c => !keycloakClientIds.Contains(c.Id))
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

