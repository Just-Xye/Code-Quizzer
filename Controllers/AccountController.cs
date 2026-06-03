using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Code_Quizzer.Models;
using Code_Quizzer.Data; // Ensure this matches the exact folder structure of AppDbContext.cs

namespace Code_Quizzer.Controllers
{
    public class AccountController : Controller
    {
        // FIX: Swapped out 'IdentityUser' for your active 'ApplicationUser' model type
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        // Inject AppDbContext and custom ApplicationUser services cleanly
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            var userScores = _context.UserScores.Where(s => s.UserId == userId).ToList();

            var viewModel = new ProfileViewModel
            {
                // Pull values directly from the database tracking properties
                ProfilePictureUrl = user.ProfilePictureUrl ?? "/uploads/avatars/default-a" +
                "vatar.png",
                Bio = user.Bio ?? "No bio available. Click edit to add yours!",
                TotalQuizzes = userScores.Count,
                CorrectAnswers = userScores.Sum(us => us.CorrectAnswers),
                WrongAnswers = userScores.Sum(us => us.TotalQuestions) - userScores.Sum(us => us.CorrectAnswers),
                TotalPoints = Math.Round(userScores.Sum(us => us.FinalScore), 1)
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (avatarFile != null && avatarFile.Length > 0)
            {
                // 1. Enforce safety checks (file extension validation)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(avatarFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Invalid image format. Only JPG, PNG, and GIF are allowed.");
                    return RedirectToAction("Profile");
                }

                // 2. Generate a completely unique filename to avoid overwriting files
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var filePath = Path.Combine(uploadFolder, fileName);

                // 3. Save the physical file to disk inside wwwroot
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                // 4. Update the current logged-in user record in the SQL database
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);

                if (user != null)
                {
                    user.ProfilePictureUrl = "/uploads/avatars/" + fileName;
                    await _userManager.UpdateAsync(user);

                    // Refresh security cookie context to apply shifts instantly
                    await _signInManager.RefreshSignInAsync(user);
                }
            }

            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateBio(string bio)
        {
            if (!string.IsNullOrWhiteSpace(bio))
            {
                HttpContext.Session.SetString("UserBio", bio);
            }
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateAvatar(string profilePictureUrl)
        {
            if (!string.IsNullOrWhiteSpace(profilePictureUrl))
            {
                HttpContext.Session.SetString("UserAvatar", profilePictureUrl);
            }
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUsername(string username)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);

                if (user != null)
                {
                    user.UserName = username;
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                    }
                }
            }
            return RedirectToAction("Profile");
        }

        // ==========================================
        // REGISTER FLOW
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // FIX: Instantiated as 'ApplicationUser' to align with SQL architecture requirements
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ==========================================
        // LOGIN FLOW
        // ==========================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.UsernameOrEmail)
                       ?? await _userManager.FindByEmailAsync(model.UsernameOrEmail);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // ==========================================
        // LEADERBOARDS
        // ==========================================

        [AllowAnonymous] // Ensures logged-out guests can view the rankings too!
        [HttpGet]
        public IActionResult Leaderboard()
        {
            // 1. Group directly by the navigation entity structure to prevent processing failures
            var leaders = _context.UserScores
                .Include(s => s.User)
                .Where(s => s.User != null) // Avoid null references
                .GroupBy(s => new { s.UserId, s.User.UserName })
                .Select(group => new
                {
                    UserId = group.Key.UserId,
                    UserObject = group.FirstOrDefault().User,
                    CumulativePoints = group.Sum(s => s.FinalScore)
                })
                .Where(x => x.CumulativePoints > 0)
                .OrderByDescending(x => x.CumulativePoints)
                .ToList();

            // 2. Project data models into your UI collection
            var leaderboardList = leaders.Select((item, index) => new LeaderboardViewModel
            {
                Rank = index + 1,
                Username = item.UserObject?.UserName ?? "Anonymous",
                ProfilePictureUrl = !string.IsNullOrEmpty(item.UserObject?.ProfilePictureUrl)
                    ? item.UserObject.ProfilePictureUrl
                    : "/uploads/avatars/default-avatar.png",
                TotalPoints = Math.Round(item.CumulativePoints, 1)
            }).ToList();

            return View("~/Views/Account/Leaderboard.cshtml", leaderboardList);
        }

        // ==========================================
        // LOGOUT FLOW
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}