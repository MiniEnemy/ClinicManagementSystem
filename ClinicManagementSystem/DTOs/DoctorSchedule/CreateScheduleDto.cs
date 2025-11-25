namespace ClinicManagementSystem.DTOs.DoctorSchedule
{
    public class CreateDoctorScheduleDto
    {
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }      // Client sends local — we convert to UTC
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime AsUtcDateOnly()
        {
            // Force UTC and return only date part
            var d = Date.Date;
            return DateTime.SpecifyKind(d, DateTimeKind.Utc);
        }
    }
}
