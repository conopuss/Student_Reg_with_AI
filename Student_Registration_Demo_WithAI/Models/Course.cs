namespace Student_Registration_Demo_WithAI.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseDescription { get; set; }
        public int Credits { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation Property
        public ICollection<Registration>? Registrations { get; set; }
    }
}
