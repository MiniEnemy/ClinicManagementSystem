using ClinicManagementSystem.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Appointment relationships
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for conflict checking using DateTime
            builder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.DateTime });

            // FIXED: Configure DayOfWeek as integer for PostgreSQL
            builder.Entity<DoctorSchedule>()
                .Property(ds => ds.DayOfWeek)
                .HasConversion<int>();

            // Index for doctor schedule lookups
            builder.Entity<DoctorSchedule>()
                .HasIndex(ds => new { ds.DoctorId, ds.DayOfWeek });
        }
    }
}