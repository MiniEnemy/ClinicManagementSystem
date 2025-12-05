using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Helpers;
using ClinicManagementSystem.DTOs.Appointment;

namespace ClinicManagementSystem.Interfaces
{
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment);
        void Update(Appointment appointment);
        Task<Appointment?> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<bool> HasConflictAsync(int doctorId, DateTime dateTime);

        // NEW METHOD ADDED: For pagination, sorting, and filtering
        Task<PagedResponse<Appointment>> GetPagedAsync(AppointmentQueryParams queryParams);
    }
}