namespace ClinicManagementSystem.DTOs.Patient
{
    public class CreatePatientDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;

        // incoming DateTime must be converted to UTC for PostgreSQL
        public DateTime DateOfBirth { get; set; }

        public DateTime ToUtcDateOfBirth()
        {
            return DateTime.SpecifyKind(DateOfBirth, DateTimeKind.Utc);
        }
    }
}
