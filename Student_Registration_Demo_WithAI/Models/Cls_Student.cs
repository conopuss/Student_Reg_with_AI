using Microsoft.EntityFrameworkCore;

namespace Student_Registration_Demo_WithAI.Models
{
    public class Cls_Student
    {
        private readonly Baglanti _context;
        public Cls_Student()
        {
             _context = new Baglanti();
        }

        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
        }
        public List<Student> GetAllStudents()
        {
            return _context.Students.Include(s=> s.Registrations).Where(s=> s.IsActive).ToList();
        }
        public Student? GetStudentByID(int id)
        {
            return _context.Students.FirstOrDefault(s=> s.StudentID == id && s.IsActive);
        }

        public Student? GetStudentByEmail(string email)
        {
            return _context.Students.FirstOrDefault(s=> s.Email == email && s.IsActive);
        }

        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
            _context.SaveChanges();
        }

        public void DeleteStudent(int id)
        {
            var student = GetStudentByID(id);
            if (student != null)
            {
                student.IsActive = false;

                var registrations = _context.Registrations.Where(s => s.StudentID == id && s.IsActive).ToList();
                foreach (var registration in registrations)
                {
                    registration.IsActive = false;
                }
                _context.SaveChanges();
            }
        }

        public int CalculateCurrentSemester(DateTime enrollmentDate)
        {
            var totalMonths = (DateTime.Now.Year - enrollmentDate.Year) * 12 + (DateTime.Now.Month - enrollmentDate.Month);
            return Math.Max(1, (totalMonths / 6) + 1); // Ensure the semester starts from 1
        }

    }
}
