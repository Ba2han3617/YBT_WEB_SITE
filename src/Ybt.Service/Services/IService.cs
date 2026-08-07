namespace Ybt.Service.Services;

public interface IService<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task RemoveAsync(int id);
}

// I'll add specific service interfaces for Blog, Event, Project etc later if needed.
