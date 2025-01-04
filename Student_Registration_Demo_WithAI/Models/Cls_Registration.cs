using Microsoft.EntityFrameworkCore;

namespace Student_Registration_Demo_WithAI.Models
{
    public class Cls_Registration
    {
        private readonly Baglanti _context;
        public Cls_Registration()
        {
            _context = new Baglanti();
        }

        public List<Registration> GetAllRegistrations()
        {
            return _context.Registrations.Include(r=> r.Student).Include(r=> r.Course).Where(r => r.IsActive).ToList();
        }
        public void RegisterForCourse(int studentID, int courseID)
        {
           
            var registration = new Registration
            {
                StudentID = studentID,
                CourseID = courseID,
                RegistrationDate = DateTime.Now,
                IsActive = true
              

                
            };
            _context.Add(registration);
            _context.SaveChanges();
        }
        public List<Registration> GetRegisteredCourses(int studentID)
        {
            return _context.Registrations.Include(r=> r.Course).Where(r=> r.StudentID == studentID && r.IsActive).ToList();
        }

        public void DeleteRegistration(int registrationID)
        {
            var registration = _context.Registrations
                .FirstOrDefault(r => r.RegistrationID == registrationID && r.IsActive);

            if (registration != null)
            {
                if (!string.IsNullOrEmpty(registration.Grade))
                {
                    throw new InvalidOperationException("Bu ders notlandığı için silinemez!");
                }

                registration.IsActive = false; // Mark the registration as inactive
                _context.SaveChanges();
            }
        }

    }
}
