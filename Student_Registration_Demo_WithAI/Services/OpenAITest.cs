using System.Net.Http;
using System.Text;
using System.Text.Json;
using Student_Registration_Demo_WithAI.Models;
using System.Threading.Tasks;

namespace Student_Registration_Demo_WithAI.Services
{
    public class OpenAITest
    {
        private readonly string _apiKey;

        public OpenAITest(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetCourseRecommendations(string studentName, int totalCreditsEarned, int remainingCredits, string currentSemester, List<string> completedCourses, List<string> availableCourses, List<Message> conversationHistory)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // Use the last user query from the conversation history
            var userQuery = conversationHistory.LastOrDefault(c => c.role == "user")?.content;

            var prompt = $@"
Student Name: {studentName}
Total Credits Earned: {totalCreditsEarned}
Remaining Credits Needed: {remainingCredits}
Current Semester: {currentSemester}
Completed Courses: {string.Join(", ", completedCourses)}
Available Courses: {string.Join(", ", availableCourses)}

User Query: {userQuery}
Continue the conversation by answering the user's query or providing recommendations as appropriate.";


            var isTurkish = IsTurkishLanguage(studentName, completedCourses, availableCourses);
            if (isTurkish)
            {
                prompt = $@"
Öğrenci Adı: {studentName}
Tamamlanan Krediler: {totalCreditsEarned}
Kalan Krediler: {remainingCredits}
Mevcut Dönem: {currentSemester}
Tamamlanan Dersler: {string.Join(", ", completedCourses)}
Mevcut Dersler: {string.Join(", ", availableCourses)}

Kullanıcı Sorusu: {userQuery}
Soruyu yanıtlayın veya uygun ders önerileri sunun.";
            }

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = conversationHistory.Select(c => new { c.role, c.content }).ToArray(),
                max_tokens = 500,
                temperature = 0.7
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

                var completion = result
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return completion.Trim();
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error calling OpenAI API: {response.StatusCode}. Response: {error}");
        }


        private bool IsTurkishLanguage(string studentName, List<string> completedCourses, List<string> availableCourses)
        {
            // Simple logic: Check if completed or available courses contain Turkish-specific characters
            var turkishCharacters = new[] { 'ç', 'ğ', 'ı', 'ö', 'ş', 'ü', 'Ç', 'Ğ', 'İ', 'Ö', 'Ş', 'Ü' };
            return completedCourses.Any(c => c.Any(turkishCharacters.Contains)) ||
                   availableCourses.Any(c => c.Any(turkishCharacters.Contains)) ||
                   studentName.Any(turkishCharacters.Contains);
        }
    }
}
