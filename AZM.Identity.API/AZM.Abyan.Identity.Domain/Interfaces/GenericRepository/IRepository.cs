using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;

public interface IRepository<TEntity, TId> where TEntity : class
{
    IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>>? predicate = null);
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task CreateManyAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void UpdateMulti(List<TEntity> entities);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
    void DeleteWhere(Expression<Func<TEntity, bool>> predicate);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IQueryable<TEntity> FromSQL(FormattableString query);
    Task<int> ExecuteSQLAsync(FormattableString query, CancellationToken cancellationToken = default);
    Task<IList<T>> SqlQueryAsync<T>(FormattableString queryString, CancellationToken cancellationToken = default) where T : class;
    Task<T?> SqlQuerySingleFirstAsync<T>(FormattableString queryString, CancellationToken cancellationToken = default) where T : class;
    void Detach(TEntity entity);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}