# Organization CRUD Mediator Implementation

## Overview
Complete CRUD mediator implementation for Organization management with simultaneous Keycloak API and database persistence operations.

## Architecture Components

### Commands (Application Layer)
1. **CreateOrganizationCommand** - Creates organization in both Keycloak and database
   - Handler: `CreateOrganizationCommandHandler`
   - Creates Keycloak organization first, then saves Tenant entity

2. **UpdateOrganizationCommand** - Updates organization in both systems
   - Handler: `UpdateOrganizationCommandHandler`
   - Updates Keycloak organization and syncs Tenant entity

3. **DeleteOrganizationCommand** - Deletes organization (soft delete in DB)
   - Handler: `DeleteOrganizationCommandHandler`
   - Deletes from Keycloak and marks Tenant as deleted

### Queries (Application Layer)
1. **GetAllOrganizationsQuery** - Retrieves all organizations
   - Handler: `GetAllOrganizationsQueryHandler`
   - Fetches from Keycloak API

2. **GetOrganizationByIdQuery** - Retrieves specific organization
   - Handler: `GetOrganizationByIdQueryHandler`
   - Fetches from Keycloak API

### Services (Application Layer)
- **IOrganizationService / OrganizationService**
  - Wraps Keycloak API calls for organization operations
  - Handles member management (add/remove from organization)
  - Used by controller for member operations

### Repository (Persistence Layer)
- **ITenantRepository / TenantRepository**
  - Implements CRUD operations for Tenant entity
  - Supports soft delete with `SoftDelete()` method
  - Queries: GetByIdAsync, GetAllAsync, GetTenantByNameAsync, GetActiveTenants

### Controller (API Layer)
- **OrganizationController**
  - Integrated with MediatR for CRUD operations
  - Uses OrganizationService for member management
  - All operations handle both Keycloak and database persistence

## Flow Diagram

```
HTTP Request (Create/Update/Delete)
    ?
OrganizationController
    ?
MediatR Command
    ?
CommandHandler (Service Layer)
    ??? IKeycloakService (Keycloak API)
    ??? ITenantRepository (Database)
    ?
Result<T> Response
```

## Dependency Injection
Registered in `DependencyInjection.cs`:
```csharp
services.AddScoped<IOrganizationService, OrganizationService>();
services.AddScoped<ITenantRepository, TenantRepository>();
```

## API Endpoints

### CRUD Operations
- `GET /api/realms/{realm}/organization` - Get all organizations
- `GET /api/realms/{realm}/organization/{id}` - Get by ID
- `POST /api/realms/{realm}/organization` - Create
- `PUT /api/realms/{realm}/organization/{id}` - Update
- `DELETE /api/realms/{realm}/organization/{id}` - Delete

### Member Management
- `GET /api/realms/{realm}/organization/{id}/members` - Get members
- `POST /api/realms/{realm}/organization/{id}/members` - Add member
- `DELETE /api/realms/{realm}/organization/{id}/members/{memberId}` - Remove member

## Database Entity
- **Tenant** - Represents organization in database
  - Maps to Organization in Keycloak
  - Soft delete implementation
  - Audit fields (CreatedAt, UpdatedAt, DeletedAt, etc.)

## Key Features
? Synchronized Keycloak and Database operations
? CQRS pattern with MediatR
? Soft delete support
? Comprehensive error handling
? Localized error messages
? Member management
? Search functionality
