using ClinicManagementSystem.Data;
using ClinicManagementSystem.Interfaces;

namespace ClinicManagementSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IPatientRepository Patients { get; }
        public IDoctorRepository Doctors { get; }
        public IAppointmentRepository Appointments { get; }
        public IDoctorScheduleRepository DoctorSchedules { get; }

        public UnitOfWork(AppDbContext context,
                          IPatientRepository patientRepo,
                          IDoctorRepository doctorRepo,
                          IAppointmentRepository appointmentRepo,
                          IDoctorScheduleRepository scheduleRepo)
        {
            _context = context;
            Patients = patientRepo;
            Doctors = doctorRepo;
            Appointments = appointmentRepo;
            DoctorSchedules = scheduleRepo;
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
