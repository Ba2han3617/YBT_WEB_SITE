using Ybt.Core.Interfaces;

namespace Ybt.Service.Services;

public class Service<T> : IService<T> where T : class
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IRepository<T> _repository;

    public Service(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = _unitOfWork.GetRepository<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _repository.GetAllAsync();

    public virtual async Task AddAsync(T entity)
    {
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public virtual async Task RemoveAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            _repository.Remove(entity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
