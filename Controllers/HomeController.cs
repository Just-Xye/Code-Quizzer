using Code_Quizzer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Code_Quizzer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var languages = new List<LanguageOptions>
            {
                new LanguageOptions { Name = "C", Code = "c" },
                new LanguageOptions { Name = "C++", Code = "cpp" },
                new LanguageOptions { Name = "C#", Code = "csharp" },
                new LanguageOptions { Name = "Java", Code = "java" },
                new LanguageOptions { Name = "Python", Code = "python" },
                new LanguageOptions { Name = "JavaScript", Code = "javascript" }
            };

            return View(languages);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
