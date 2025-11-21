namespace ClinicManagementSystem.DTOs.Doctor
{
    public class CreateDoctorDto
    {
        public string FullName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
