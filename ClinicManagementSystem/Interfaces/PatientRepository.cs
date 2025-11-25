using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;

        public PatientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Patient entity)
        {
            await _context.Patients.AddAsync(entity);
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public void Update(Patient entity)
        {
            _context.Patients.Update(entity);
        }

        public void Remove(Patient entity)
        {
            _context.Patients.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var patient = await GetByIdAsync(id);
            if (patient != null)
            {
                patient.IsDeleted = true;
                _context.Patients.Update(patient);
            }
        }
    }
}
