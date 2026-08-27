using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.IRepository;

namespace StockFlow.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly AppDbContext context;

    public GenericRepository(AppDbContext _context) {
       context=_context;
    }
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        context.Set<TEntity>().Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
