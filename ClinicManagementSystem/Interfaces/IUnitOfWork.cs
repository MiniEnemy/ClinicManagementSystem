using System.Threading.Tasks;

namespace ClinicManagementSystem.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPatientRepository Patients { get; }
        IDoctorRepository Doctors { get; }
        IAppointmentRepository Appointments { get; }
        IDoctorScheduleRepository DoctorSchedules { get; }
        Task<int> CompleteAsync();
    }
}
