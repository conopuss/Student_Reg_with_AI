namespace Student_Registration_Demo_WithAI.Models
{
    public class Student
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation Property
        public ICollection<Registration>? Registrations { get; set; }
    }
}
