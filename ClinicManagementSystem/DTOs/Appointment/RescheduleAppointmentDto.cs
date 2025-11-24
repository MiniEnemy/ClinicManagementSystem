namespace ClinicManagementSystem.DTOs.Appointment
{
    public class RescheduleAppointmentDto
    {
        public DateTime NewDate { get; set; }
        public string? Description { get; set; }
    }
}
