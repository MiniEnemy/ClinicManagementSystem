namespace ClinicManagementSystem.Entities
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public void SetUtcDates()
        {
            DateOfBirth = DateTime.SpecifyKind(DateOfBirth, DateTimeKind.Utc);
            CreatedAt = DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc);
        }
    }

}
