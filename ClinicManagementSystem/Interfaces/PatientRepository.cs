using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;
        public PatientRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Patient entity) => await _context.Patients.AddAsync(entity);

        public async Task<IEnumerable<Patient>> GetAllAsync() =>
            await _context.Patients.Where(p => !p.IsDeleted).ToListAsync();

        public async Task<Patient?> GetByIdAsync(int id) =>
            await _context.Patients.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        public void Remove(Patient entity) => _context.Patients.Remove(entity);

        public void Update(Patient entity) => _context.Patients.Update(entity);

        public async Task SoftDeleteAsync(int id)
        {
            var p = await _context.Patients.FindAsync(id);
            if (p != null) p.IsDeleted = true;
        }
    }
}
