
using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories
{
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly AppDbContext _context;

        public DoctorScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DoctorSchedule entity)
        {
            await _context.DoctorSchedules.AddAsync(entity);
        }

        public async Task<IEnumerable<DoctorSchedule>> GetAllAsync()
        {
            return await _context.DoctorSchedules
                .Include(ds => ds.Doctor)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctorId)
                .OrderBy(ds => ds.DayOfWeek)
                .ThenBy(ds => ds.StartTime)
                .ToListAsync();
        }

        public async Task<DoctorSchedule?> GetByIdAsync(int id)
        {
            return await _context.DoctorSchedules
                .Include(ds => ds.Doctor)
                .FirstOrDefaultAsync(ds => ds.Id == id);
        }

        public void Update(DoctorSchedule entity)
        {
            _context.DoctorSchedules.Update(entity);
        }

        public void Remove(DoctorSchedule entity)
        {
            _context.DoctorSchedules.Remove(entity);
        }

        // FIXED: Updated for DayOfWeek system
        public async Task<bool> HasOverlapAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end)
        {
            var dayOfWeek = date.DayOfWeek;
            return await _context.DoctorSchedules.AnyAsync(ds =>
                ds.DoctorId == doctorId &&
                ds.DayOfWeek == dayOfWeek &&
                ds.StartTime < end &&
                ds.EndTime > start
            );
        }

        // FIXED: Updated for DayOfWeek system
        public async Task<bool> HasOverlapAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end, int excludeId)
        {
            var dayOfWeek = date.DayOfWeek;
            return await _context.DoctorSchedules.AnyAsync(ds =>
                ds.DoctorId == doctorId &&
                ds.Id != excludeId &&
                ds.DayOfWeek == dayOfWeek &&
                ds.StartTime < end &&
                ds.EndTime > start
            );
        }

        // NEW: Get schedule for specific day
        public async Task<DoctorSchedule?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek dayOfWeek)
        {
            return await _context.DoctorSchedules
                .FirstOrDefaultAsync(ds => ds.DoctorId == doctorId && ds.DayOfWeek == dayOfWeek);
        }
    }
}