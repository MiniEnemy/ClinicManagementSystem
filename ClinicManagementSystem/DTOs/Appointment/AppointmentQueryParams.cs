using System;
using System.ComponentModel.DataAnnotations;
using ClinicManagementSystem.Helpers;

namespace ClinicManagementSystem.DTOs.Appointment
{
    public class AppointmentQueryParams : PaginationParams
    {
        // Sorting
        public string SortBy { get; set; } = "AppointmentDate";
        public string SortDirection { get; set; } = "asc";

        // Filtering
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }
}