using Microsoft.AspNetCore.Identity;

namespace ClinicManagementSystem.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? DoctorId { get; set; }
    }
}
