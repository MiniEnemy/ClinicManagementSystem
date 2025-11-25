using System;

namespace ClinicManagementSystem.Entities
{
    public enum AppointmentStatus { Scheduled, Completed, Cancelled }

    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        public DateTime DateTime { get; set; } // single UTC DateTime

        public string? Description { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
