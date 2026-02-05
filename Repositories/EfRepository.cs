using Microsoft.EntityFrameworkCore;
using Api.Comercial.Data;

namespace Api.Comercial.Repositories;

public interface IRepository<TEntity, TKey>
    where TEntity : class
{
    IQueryable<TEntity> Query(bool asNoTracking = false);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    private readonly ApeironDbContext _context;
    private readonly DbSet<TEntity> _set;

    public EfRepository(ApeironDbContext context)
    {
        _context = context;
        _set = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query(bool asNoTracking = false)
        => asNoTracking ? _set.AsNoTracking() : _set;

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
        => _set.FindAsync(new object[] { id! }, cancellationToken).AsTask();

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        => _set.AddAsync(entity, cancellationToken).AsTask();

    public void Remove(TEntity entity)
        => _set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
