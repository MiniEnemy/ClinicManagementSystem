using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task SoftDeleteAsync(int id);
    }
}
