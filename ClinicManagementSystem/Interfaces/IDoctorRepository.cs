using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        // Return a Doctor object (nullable) by email
        Task<Doctor?> GetByEmailAsync(string email);
    }
}
