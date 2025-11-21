using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate);
    }
}
