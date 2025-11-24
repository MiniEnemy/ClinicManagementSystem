namespace ClinicManagementSystem.DTOs.Patient
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string FullName => FirstName + " " + LastName;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
