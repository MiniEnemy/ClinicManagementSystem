using ClinicManagementSystem.Data;
using ClinicManagementSystem.Entities;
using ClinicManagementSystem.Interfaces;
using ClinicManagementSystem.Helpers;
using ClinicManagementSystem.DTOs.Appointment;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace ClinicManagementSystem.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        public void Update(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int doctorId, DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();

            return await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId && a.DateTime == utcDateTime);
        }

        // NEW METHOD: For pagination, sorting, and filtering
        public async Task<PagedResponse<Appointment>> GetPagedAsync(AppointmentQueryParams queryParams)
        {
            // Step 1: Start building query (IQueryable - NOT executed yet)
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            // Step 2: Apply filters based on query parameters
            if (queryParams.PatientId.HasValue)
            {
                query = query.Where(a => a.PatientId == queryParams.PatientId.Value);
            }

            if (queryParams.DoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == queryParams.DoctorId.Value);
            }

            if (!string.IsNullOrEmpty(queryParams.PatientName))
            {
                query = query.Where(a =>
                    (a.Patient.FirstName + " " + a.Patient.LastName)
                    .ToLower()
                    .Contains(queryParams.PatientName.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParams.DoctorName))
            {
                query = query.Where(a =>
                    a.Doctor.FullName.ToLower()
                    .Contains(queryParams.DoctorName.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParams.Status))
            {
                if (Enum.TryParse<AppointmentStatus>(queryParams.Status, true, out var status))
                {
                    query = query.Where(a => a.Status == status);
                }
            }

            // FIX: Handle DateTime conversion for PostgreSQL
            if (queryParams.StartDate.HasValue)
            {
                var startDate = queryParams.StartDate.Value;
                // Convert to UTC if unspecified
                if (startDate.Kind == DateTimeKind.Unspecified)
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                else
                    startDate = startDate.ToUniversalTime();

                query = query.Where(a => a.DateTime >= startDate);
            }

            if (queryParams.EndDate.HasValue)
            {
                var endDate = queryParams.EndDate.Value;
                // Convert to UTC if unspecified
                if (endDate.Kind == DateTimeKind.Unspecified)
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                else
                    endDate = endDate.ToUniversalTime();

                query = query.Where(a => a.DateTime <= endDate);
            }

            if (queryParams.CreatedAfter.HasValue)
            {
                var createdAfter = queryParams.CreatedAfter.Value;
                // Convert to UTC if unspecified
                if (createdAfter.Kind == DateTimeKind.Unspecified)
                    createdAfter = DateTime.SpecifyKind(createdAfter, DateTimeKind.Utc);
                else
                    createdAfter = createdAfter.ToUniversalTime();

                query = query.Where(a => a.CreatedAt >= createdAfter);
            }

            if (queryParams.CreatedBefore.HasValue)
            {
                var createdBefore = queryParams.CreatedBefore.Value;
                // Convert to UTC if unspecified
                if (createdBefore.Kind == DateTimeKind.Unspecified)
                    createdBefore = DateTime.SpecifyKind(createdBefore, DateTimeKind.Utc);
                else
                    createdBefore = createdBefore.ToUniversalTime();

                query = query.Where(a => a.CreatedAt <= createdBefore);
            }

            // Step 3: Count total records (executes COUNT query)
            var totalRecords = await query.CountAsync();

            // Step 4: Apply sorting
            if (!string.IsNullOrEmpty(queryParams.SortBy))
            {
                var sortDirection = queryParams.SortDirection.ToLower() == "desc" ? "descending" : "ascending";

                // Map DTO field names to entity field names
                var sortField = queryParams.SortBy switch
                {
                    "PatientName" => "Patient.FirstName",
                    "DoctorName" => "Doctor.FullName",
                    "AppointmentDate" => "DateTime",
                    _ => queryParams.SortBy
                };

                try
                {
                    query = query.OrderBy($"{sortField} {sortDirection}");
                }
                catch
                {
                    // Fallback to default sorting
                    query = query.OrderByDescending(a => a.DateTime);
                }
            }
            else
            {
                query = query.OrderByDescending(a => a.DateTime);
            }

            // Step 5: Apply pagination (SKIP and TAKE)
            var appointments = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            // Step 6: Return packaged response
            return new PagedResponse<Appointment>(
                appointments,
                queryParams.PageNumber,
                queryParams.PageSize,
                totalRecords
            );
        }
    }
}