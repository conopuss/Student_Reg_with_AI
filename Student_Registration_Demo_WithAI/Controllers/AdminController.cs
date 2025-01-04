using Microsoft.AspNetCore.Mvc;
using Student_Registration_Demo_WithAI.Models;

namespace Student_Registration_Demo_WithAI.Controllers
{
    public class AdminController : Controller
    {
       
        private readonly Cls_Student cls_Student = new Cls_Student();
        private readonly Cls_Courses cls_Courses = new Cls_Courses();
        private readonly Cls_Registration cls_Registration = new Cls_Registration();

       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("AdminLogin", "Home");
        }


        // Manage Students
        public IActionResult Students()
        {
            var students = cls_Student.GetAllStudents();
            return View(students);
        }

   

        // Create Student (GET and POST)
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(Student student)
        {
            if (ModelState.IsValid)
            {
                student.IsActive = true; // Ensure the student is marked as active
                cls_Student.AddStudent(student); // Assuming this method saves the student to the database
                TempData["Message"] = "Öğrenci başarıyla eklendi.";
                return RedirectToAction("Students");
            }

            TempData["Error"] = "Öğrenci eklenirken bir hata oluştu.";
            return View(student);
        }

        public IActionResult EditStudentRegistrations(int id)
        {
            var registrations = cls_Registration.GetRegisteredCourses(id);

            ViewBag.AvailableCourses = cls_Courses.GetAvailableCourses(id);

            ViewBag.StudentID = id;
            return View(registrations);

        }

        

        // Edit Student (GET and POST)
        public IActionResult EditStudent(int id)
        {
            var student = cls_Student.GetStudentByID(id);
            if (student == null)
            {
                return NotFound();
            }

            ViewBag.Info = student; // For displaying student info in the view
            return View(student);   // Pass the Student object to the view
        }


        [HttpPost]
        public IActionResult EditStudent(Student model)
        {
            if (ModelState.IsValid)
            {
                cls_Student.UpdateStudent(model); // Update student details

                TempData["Message"] = "Öğrenci bilgileri başarıyla güncellendi.";
                return RedirectToAction("EditStudent", new { id = model.StudentID });
            }

            TempData["Message"] = "Öğrenci bilgileri güncellenirken bir hata oluştu.";
            return View(model);
        }



        // Delete Student
        public IActionResult DeleteStudent(int id)
        {
            cls_Student.DeleteStudent(id);
            return RedirectToAction("Students");
        }


        // Manage Courses
        public IActionResult Courses()
        {
            var courses = cls_Courses.GetAllCourses();
            return View(courses);
        }

        // Create Course (GET and POST)
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                cls_Courses.AddCourse(course);
                return RedirectToAction("Courses");
            }
            return View(course);
        }

        // Edit Course (GET and POST)
        public IActionResult EditCourse(int id)
        {
            var course = cls_Courses.GetCourseByID(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        public IActionResult EditCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                cls_Courses.UpdateCourse(course);
                return RedirectToAction("Courses");
            }
            return View(course);
        }

        // Delete Course
        public IActionResult DeleteCourse(int id)
        {
            cls_Courses.DeleteCourse(id);
            return RedirectToAction("Courses");
        }


        // Manage Registrations
        public IActionResult Registrations()
        {
            var registrations = cls_Registration.GetAllRegistrations();
            return View(registrations);
        }

        [HttpPost]
        public IActionResult AddRegistration(int CourseID, int StudentID)
        {
            // Check if the student is already registered for the course
            var existingRegistration = cls_Registration.GetRegisteredCourses(StudentID)
                                                       .FirstOrDefault(r => r.CourseID == CourseID);

            if (existingRegistration != null)
            {
                TempData["Message"] = "Öğrenci bu derse zaten kayıtlı.";
                return RedirectToAction("EditStudentRegistrations", new { id = StudentID });
            }

            // Register the student for the course
            cls_Registration.RegisterForCourse(StudentID, CourseID);
            TempData["Message"] = "Ders başarıyla eklendi.";

            // Redirect back to the EditStudent page
            return RedirectToAction("EditStudentRegistrations", new { id = StudentID });
        }


        // Delete Registration
        public IActionResult DeleteRegistration(int id, int studentID)
        {
            try
            {
                cls_Registration.DeleteRegistration(id);
                TempData["Message"] = "Kayıt başarıyla silindi.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = ex.Message; // Display the exception message to the user
            }
            catch (Exception)
            {
                TempData["Message"] = "Bir hata oluştu, lütfen tekrar deneyin.";
            }

            return RedirectToAction("Registrations", new { id = studentID });
        }


    }
}
