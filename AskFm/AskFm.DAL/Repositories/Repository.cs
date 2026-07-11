using System.Linq.Expressions;
using AskFm.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AskFm.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }


    public IQueryable<T> GetAll(bool trackChanges = true) => 
        trackChanges ? _dbSet.AsQueryable() : _dbSet.AsNoTracking();

    public async Task<IEnumerable<T>> GetAllAsync(bool trackChanges = true) => 
        trackChanges ? await _dbSet.ToListAsync() : await _dbSet.AsNoTracking().ToListAsync();


    public T? GetById(int id) => _dbSet.Find(id);
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);


    public IQueryable<T> FindAll(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true)
    {
        IQueryable<T> query = _dbSet;
        
        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return query.Where(predicate);
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true)
    {
        IQueryable<T> query = _dbSet;

        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes != null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.Where(predicate).ToListAsync();
    }


    public T? Find(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true)
    {
        IQueryable<T> query = _dbSet;

        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return query.FirstOrDefault(predicate);
    }

    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true)
    {
        IQueryable<T> query = _dbSet;

        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.FirstOrDefaultAsync(predicate);
    }


    public void Add(T entity) => _dbSet.Add(entity);

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);


    public void Update(T entity) => _dbSet.Update(entity);

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }


    public void Remove(T entity) => _dbSet.Remove(entity);

    public Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }

    public async Task<List<T>> GetPagedAsync(
        int skip,
        int take,
        Expression<Func<T, object>> orderBy,
        bool ascending = true,
        Expression<Func<T, bool>>? predicate = null,
        string[]? includes = null,
        bool trackChanges = true)
    {
        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be non-negative");
        if (take <= 0)
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive");
        IQueryable<T> query = _dbSet;

        if (!trackChanges)
            query = query.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (ascending)
        {
            query = query.OrderBy(orderBy);
        }
        else
        {
            query = query.OrderByDescending(orderBy);
        }

        return await query.Skip(skip).Take(take).ToListAsync();
    }
}