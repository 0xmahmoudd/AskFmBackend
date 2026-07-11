using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AskFm.DAL.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll(bool trackChanges = true);
    Task<IEnumerable<T>> GetAllAsync(bool trackChanges = true);


    T? GetById(int id);
    Task<T?> GetByIdAsync(int id);


    IQueryable<T> FindAll(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate = null, string[] includes = null, bool trackChanges = true);



    T? Find(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[] includes = null, bool trackChanges = true);


    void Add(T entity);
    Task AddAsync(T entity);


    void Update(T entity);
    Task UpdateAsync(T entity);


    void Remove(T entity);
    Task RemoveAsync(T entity);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    Task<List<T>> GetPagedAsync(
        int skip,
        int take,
        Expression<Func<T, object>> orderBy,
        bool ascending = true,
        Expression<Func<T, bool>>? predicate = null,
        string[]? includes = null,
        bool trackChanges = true);
}