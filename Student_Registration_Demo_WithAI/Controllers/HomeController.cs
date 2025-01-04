using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Student_Registration_Demo_WithAI.Models;
using Student_Registration_Demo_WithAI.Services;

namespace Student_Registration_Demo_WithAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly Cls_Student cls_Student = new Cls_Student();
        private readonly Cls_Registration cls_Registration = new Cls_Registration();
        private readonly Cls_Courses cls_Courses = new Cls_Courses();

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var student = cls_Student.GetStudentByEmail(Email);
            if (student == null || student.PasswordHash != Password)
            {
                ViewBag.Error = "Geçersiz giriþ";
                return View();
            }

            HttpContext.Session.SetInt32("StudentID", student.StudentID);
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(string Email, string Password)
        {
            if (Email == "admin@email.com" && Password == "123")
            {
                HttpContext.Session.SetString("UserType", "Admin");
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Geçersiz yönetici bilgileri.";
            return View();
        }

        public IActionResult Index()
        {
            int? studentID = HttpContext.Session.GetInt32("StudentID");
            if (studentID == null)
            {
                return RedirectToAction("Login");
            }
            var student = cls_Student.GetStudentByID(studentID.Value);
            if (student == null || !student.IsActive)
            {
                return NotFound();
            }
            return View(student);
        }

        public IActionResult Courses()
        {
            int? studentID = HttpContext.Session.GetInt32("StudentID");
            if (studentID == null) return RedirectToAction("Login");

            var courses = cls_Courses.GetAvailableCourses(studentID.Value);
            return View(courses);
        }

        [HttpPost]
        public IActionResult RegisterCourse(int courseID)
        {
            int? studentID = HttpContext.Session.GetInt32("StudentID");
            if (studentID == null) return RedirectToAction("Login");

            cls_Registration.RegisterForCourse(studentID.Value, courseID);
            TempData["Message"] = "You have successfully registered for the course!";
            return RedirectToAction("Courses");
        }

        public IActionResult RegisteredCourses()
        {
            int? studentID = HttpContext.Session.GetInt32("StudentID");
            if (studentID == null) return RedirectToAction("Login");

            var student = cls_Student.GetStudentByID(studentID.Value);
            if (student == null || !student.IsActive) return NotFound();

            // Calculate the current semester
            var currentSemester = cls_Student.CalculateCurrentSemester(student.EnrollmentDate);

            ViewBag.CurrentSemester = currentSemester;

            var registrations = cls_Registration.GetRegisteredCourses(studentID.Value)
                .Where(r => r.IsActive)
                .ToList();

            return View(registrations);
        }


        public IActionResult CourseDetails(int id)
        {
            var details = cls_Courses.GetCourseByID(id);
            if (details == null) return NotFound();
            return View(details);
        }

        public IActionResult GenerateRecommendations()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> GenerateRecommendations(string userQuery)
        {
            int? studentID = HttpContext.Session.GetInt32("StudentID");
            if (studentID == null) return RedirectToAction("Login");

            var student = cls_Student.GetStudentByID(studentID.Value);
            if (student == null || !student.IsActive) return NotFound();

            string currentSemester = cls_Student.CalculateCurrentSemester(student.EnrollmentDate).ToString();
            var registrations = cls_Registration.GetRegisteredCourses(studentID.Value)
                                                 .Where(r => !string.IsNullOrEmpty(r.Grade) && r.Grade.ToUpper() != "F")
                                                 .ToList();

            int totalCompletedCredits = registrations.Sum(r => r.Course?.Credits ?? 0);
            int totalCreditsRequired = 60; // Example total credits for graduation
            int remainingCredits = totalCreditsRequired - totalCompletedCredits;

            var completedCourses = registrations.Select(r => r.Course?.CourseName).Where(c => !string.IsNullOrEmpty(c)).ToList();
            var availableCourses = cls_Courses.GetAvailableCourses(studentID.Value).Select(c => c.CourseName).ToList();

            try
            {
                var apiKey = "OPENAI_API_KEY"; // Replace with your actual API key
                var openAITest = new OpenAITest(apiKey);

                // Retrieve conversation history from session
                var conversationHistoryJson = HttpContext.Session.GetString("ConversationHistory");
                var conversationHistory = !string.IsNullOrEmpty(conversationHistoryJson)
                    ? JsonSerializer.Deserialize<List<Message>>(conversationHistoryJson)
                    : new List<Message>();

                if (!conversationHistory.Any())
                {
                    conversationHistory.Add(new Message
                    {
                        role = "system",
                        content = "You are a helpful assistant specialized in academic advising. Respond in Turkish or English based on the user's input."
                    });

                    conversationHistory.Add(new Message
                    {
                        role = "user",
                        content = $@"
Öðrenci Adý: {student.FirstName} {student.LastName}
Tamamlanan Krediler: {totalCompletedCredits}
Kalan Krediler: {remainingCredits}
Mevcut Dönem: {currentSemester}
Tamamlanan Dersler: {string.Join(", ", completedCourses)}
Mevcut Dersler: {string.Join(", ", availableCourses)}
"
                    });

                    conversationHistory.Add(new Message
                    {
                        role = "assistant",
                        content = "Bu bilgiler temel alýnarak size yardýmcý olacaðým. Ne öðrenmek istersiniz?"
                    });
                }

                // Add user query to the conversation history
                conversationHistory.Add(new Message { role = "user", content = userQuery });

                // Clean and persist the conversation history
                conversationHistory = conversationHistory
                    .Where(entry => !string.IsNullOrWhiteSpace(entry.content))
                    .ToList();

                var recommendations = await openAITest.GetCourseRecommendations(
                    $"{student.FirstName} {student.LastName}",
                    totalCompletedCredits,
                    remainingCredits,
                    currentSemester,
                    completedCourses,
                    availableCourses,
                    conversationHistory
                );

                recommendations = FormatResponse(recommendations);
                // Save updated conversation history to session
                HttpContext.Session.SetString("ConversationHistory", JsonSerializer.Serialize(conversationHistory));

                ViewBag.Recommendations = recommendations;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Öneri oluþturulurken bir hata oluþtu: " + ex.Message;
            }

            return View();
        }

        private string FormatResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return response;

            // Add line breaks for long text to make it more readable
            return response.Replace(". ", ".\n");
        }




    }
}
