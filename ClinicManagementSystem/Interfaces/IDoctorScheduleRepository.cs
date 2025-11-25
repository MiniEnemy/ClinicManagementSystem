using ClinicManagementSystem.Entities;

namespace ClinicManagementSystem.Interfaces
{
    public interface IDoctorScheduleRepository
    {
        Task AddAsync(DoctorSchedule entity);
        Task<IEnumerable<DoctorSchedule>> GetAllAsync();
        Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);
        Task<DoctorSchedule?> GetByIdAsync(int id);
        void Update(DoctorSchedule entity);
        void Remove(DoctorSchedule entity);

        // For creating new schedule
        Task<bool> HasOverlapAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end);

        // For updating existing schedule (exclude that schedule)
        Task<bool> HasOverlapAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end, int excludeId);
    }
}
