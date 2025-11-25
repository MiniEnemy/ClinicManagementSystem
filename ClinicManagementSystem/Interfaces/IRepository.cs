using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Interfaces
{
    public interface IRepository<T>
    {
        Task AddAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        void Update(T entity);
        void Remove(T entity);
    }
}
