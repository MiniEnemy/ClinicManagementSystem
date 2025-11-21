using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _context;
        public DoctorRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Doctor entity) => await _context.Doctors.AddAsync(entity);
        public async Task<IEnumerable<Doctor>> GetAllAsync() => await _context.Doctors.ToListAsync();
        public async Task<Doctor?> GetByIdAsync(int id) => await _context.Doctors.FindAsync(id);
        public void Remove(Doctor entity) => _context.Doctors.Remove(entity);
        public void Update(Doctor entity) => _context.Doctors.Update(entity);
    }
}
