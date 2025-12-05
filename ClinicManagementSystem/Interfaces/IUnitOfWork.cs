using ClinicManagementSystem.Interfaces;

namespace ClinicManagementSystem.Data
{
    public interface IUnitOfWork
    {
        IAppointmentRepository Appointments { get; }
        IPatientRepository Patients { get; }
        IDoctorRepository Doctors { get; }
        IDoctorScheduleRepository DoctorSchedules { get; }

        Task<int> CompleteAsync();
    }
}