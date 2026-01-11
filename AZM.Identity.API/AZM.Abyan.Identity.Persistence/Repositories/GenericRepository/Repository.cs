using System.Linq.Expressions;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AZM.Abyan.Identity.Persistence.Repositories.GenericRepository;

public class Repository<TEntity, TId, TContext> : IRepository<TEntity, TId>
   where TEntity : class
   where TContext : DbContext
{
    protected readonly TContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(TContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate != null ? _dbSet.Where(predicate) : _dbSet;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id! }, cancellationToken: cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var property = Expression.Property(parameter, "Id");
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TId));

        var containsExpression = Expression.Call(
            containsMethod,
            Expression.Constant(ids),
            property
        );

        var lambda = Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);

        return await _dbSet.Where(lambda).ToListAsync(cancellationToken);
    }

    public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task CreateManyAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateMulti(List<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual void DeleteWhere(Expression<Func<TEntity, bool>> predicate)
    {
        var entities = GetWhere(predicate);
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<TEntity> FromSQL(FormattableString query)
    {
        return _dbSet.FromSqlInterpolated(query);
    }

    public async Task<int> ExecuteSQLAsync(FormattableString query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.ExecuteSqlInterpolatedAsync(query, cancellationToken);
    }

    public async Task<IList<T>> SqlQueryAsync<T>(FormattableString queryString, CancellationToken cancellationToken = default) where T : class
    {
        return await _dbContext.Database.SqlQueryRaw<T>(queryString.Format, queryString.GetArguments()!)
            .ToListAsync(cancellationToken);
    }

    public async Task<T?> SqlQuerySingleFirstAsync<T>(FormattableString queryString, CancellationToken cancellationToken = default) where T : class
    {
        return await _dbContext.Database.SqlQueryRaw<T>(queryString.Format, queryString.GetArguments()!)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual void Detach(TEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Detached;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}