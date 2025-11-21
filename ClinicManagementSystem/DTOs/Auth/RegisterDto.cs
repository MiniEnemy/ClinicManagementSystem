namespace ClinicManagementSystem.DTOs.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Role { get; set; } // Admin | Doctor | Receptionist
    }
}
