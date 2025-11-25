using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.DTOs.Appointment
{
    public class CreateAppointmentDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime DateTime { get; set; } // combined date + time

        public string? Description { get; set; }
    }
}
