using System;

namespace ClinicManagementSystem.DTOs.Appointment
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime DateTime { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Scheduled";
        public DateTime CreatedAt { get; set; }

        // Navigation properties for display
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientEmail { get; set; }
        public string? DoctorEmail { get; set; }
        public string? DoctorSpecialization { get; set; }
    }
}