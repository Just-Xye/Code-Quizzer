using Code_Quizzer.Data;
using Code_Quizzer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // 1. Get the current logged-in user's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. Fetch the quiz history records for this user from the database
        var userScores = _context.UserScores
            .Where(us => us.UserId == userId)
            .ToList();

        // 3. Calculate all the aggregate stats needed for the dashboard cards
        int totalQuizzes = userScores.Count;
        int correctAnswers = userScores.Sum(us => us.CorrectAnswers);
        int totalQuestions = userScores.Sum(us => us.TotalQuestions);
        int wrongAnswers = totalQuestions - correctAnswers;

        // 4. Retrieve saved Bio and Avatar text strings from temporary session caches
        string userBio = HttpContext.Session.GetString("UserBio") ?? "No bio available. Click edit to add yours!";
        string avatarUrl = HttpContext.Session.GetString("UserAvatar") ?? "";

        // 5. PACK EVERYTHING INTO THE CORRECT MODEL TYPE!
        var viewModel = new ProfileViewModel
        {
            ProfilePictureUrl = avatarUrl,
            Bio = userBio,
            TotalQuizzes = totalQuizzes,
            CorrectAnswers = correctAnswers,
            WrongAnswers = wrongAnswers
        };

        // 6. Pass the compiled ProfileViewModel down to your View
        return View("Profile", viewModel);
    }

    // 2. POST: /Profile/UpdateBio
    [HttpPost]
    public IActionResult UpdateBio(string bio)
    {
        if (!string.IsNullOrWhiteSpace(bio))
        {
            // For now, save it to Session state as a quick mock mechanism
            HttpContext.Session.SetString("UserBio", bio);
        }
        return RedirectToAction("Index");
    }

    // 3. POST: /Profile/UpdateAvatar
    [HttpPost]
    public IActionResult UpdateAvatar(string profilePictureUrl)
    {
        if (!string.IsNullOrWhiteSpace(profilePictureUrl))
        {
            HttpContext.Session.SetString("UserAvatar", profilePictureUrl);
        }
        return RedirectToAction("Index");
    }

    // 4. POST: /Profile/UpdateUsername
    [HttpPost]
    public async Task<IActionResult> UpdateUsername(string username)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            // NOTE: Changing usernames requires updating the ASP.NET Identity Cookie 
            // If you are using Identity, you would call _userManager.SetUserNameAsync() here.
            // For now, this placeholder handles routing cleanly back to profile dashboard.
        }
        return RedirectToAction("Index");
    }
}