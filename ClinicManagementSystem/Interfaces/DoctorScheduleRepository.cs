using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories
{
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly AppDbContext _context;
        public DoctorScheduleRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(DoctorSchedule entity) => await _context.DoctorSchedules.AddAsync(entity);
        public async Task<IEnumerable<DoctorSchedule>> GetAllAsync() => await _context.DoctorSchedules.Include(d => d.Doctor).ToListAsync();
        public async Task<DoctorSchedule?> GetByIdAsync(int id) => await _context.DoctorSchedules.FindAsync(id);
        public void Remove(DoctorSchedule entity) => _context.DoctorSchedules.Remove(entity);
        public void Update(DoctorSchedule entity) => _context.DoctorSchedules.Update(entity);

        public async Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId) =>
            await _context.DoctorSchedules.Where(s => s.DoctorId == doctorId).ToListAsync();
    }
}
