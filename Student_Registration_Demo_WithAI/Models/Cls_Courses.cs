using Microsoft.EntityFrameworkCore;

namespace Student_Registration_Demo_WithAI.Models
{
    public class Cls_Courses
    {
        private readonly Baglanti _context;
        public Cls_Courses()
        {
            _context = new Baglanti();
        }
        public List<Course> GetAllCourses()
        {
            return _context.Courses.Where(c => c.IsActive).ToList();
        }

      
        public List<Course> GetAvailableCourses(int studentID)
        {
            var registeredCourseIDs = _context.Registrations.Where(c=> c.StudentID == studentID && c.IsActive).Select(c=> c.CourseID).ToList();
            return _context.Courses.Where(c=> c.IsActive && !registeredCourseIDs.Contains(c.CourseID)).ToList();
        }

        public List<Registration> GetRegisteredCourses(int studentId)
        {
            return _context.Registrations
                .Include(r => r.Course) // Include course details
                .Where(r => r.StudentID == studentId && r.IsActive)
                .ToList();
        }

        public void AddCourse(Course course)
        {
            course.IsActive = true; // Set course to active by default
            _context.Courses.Add(course);
            _context.SaveChanges();
        }

        public Course GetCourseByID(int id)
        {
            return _context.Courses.FirstOrDefault(c => c.CourseID == id && c.IsActive);
        }

        // 4. Update an Existing Course
        public void UpdateCourse(Course course)
        {
            var existingCourse = _context.Courses
                .FirstOrDefault(c => c.CourseID == course.CourseID && c.IsActive);

            if (existingCourse != null)
            {
                existingCourse.CourseName = course.CourseName;
                existingCourse.CourseDescription = course.CourseDescription;
                existingCourse.Credits = course.Credits;
                existingCourse.InstructorName = course.InstructorName;

                _context.SaveChanges();
            }
        }

        // 5. Soft Delete a Course
        public void DeleteCourse(int id)
        {
            var course = _context.Courses
                .FirstOrDefault(c => c.CourseID == id && c.IsActive);

            if (course != null)
            {
                course.IsActive = false; // Mark course as inactive
                _context.SaveChanges();
            }
        }

    }
}
