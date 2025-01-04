namespace Student_Registration_Demo_WithAI.Models
{
    public class Registration
    {
        public int RegistrationID { get; set; }

        // Foreign Keys
        public int StudentID { get; set; }
        public Student? Student { get; set; }

        public int CourseID { get; set; }
        public Course? Course { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string? Grade { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
