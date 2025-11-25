using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Interfaces
{
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment);
        void Update(Appointment appointment);
        Task<Appointment?> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);

        Task<bool> HasConflictAsync(int doctorId, DateTime dateTime); // single DateTime
    }
}
