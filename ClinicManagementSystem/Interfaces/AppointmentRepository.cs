using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;
        public AppointmentRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Appointment entity) => await _context.Appointments.AddAsync(entity);

        public async Task<IEnumerable<Appointment>> GetAllAsync() =>
            await _context.Appointments.Include(a => a.Patient).Include(a => a.Doctor).ToListAsync();

        public async Task<Appointment?> GetByIdAsync(int id) =>
            await _context.Appointments.Include(a => a.Patient).Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

        public void Remove(Appointment entity) => _context.Appointments.Remove(entity);

        public void Update(Appointment entity) => _context.Appointments.Update(entity);

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId) =>
            await _context.Appointments.Where(a => a.DoctorId == doctorId).ToListAsync();

        // Simple overlap check: assume exact DateTime match is conflict (improve as needed)
        public async Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate) =>
            await _context.Appointments.AnyAsync(a => a.DoctorId == doctorId && a.AppointmentDate == appointmentDate);
    }
}
