using Code_Quizzer.Data;
using Code_Quizzer.Extensions;
using Code_Quizzer.Models;
using Code_Quizzer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class QuizController : Controller
{
    private readonly AppDbContext _context;

    public QuizController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Difficulty(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return RedirectToAction("Index", "Home");
        }

        var categoriesFromDb = _context.Questions
            .Where(q => q.Language == lang)
            .Select(q => q.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var model = new DifficultyViewModel
        {
            Language = lang,
            Categories = categoriesFromDb,
            Difficulties = new List<string> { "Easy", "Medium", "Hard" },
            QuestionCounts = new List<int> { 5, 10, 20 }
        };

        return View(model);
    }

    public IActionResult Start(string lang, string difficulty, string category, int questions)
    {
        // FRESH CLEANUP: Wipe out any leftover quiz data right before starting a new one
        HttpContext.Session.Remove("QuizQuestions");
        HttpContext.Session.Remove("QuizAnswers");

        var query = _context.Questions.Include(q => q.Choices).AsQueryable();

        query = query.Where(q => q.Language.Trim().ToLower() == lang.Trim().ToLower() &&
                                 q.Difficulty.Trim().ToLower() == difficulty.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(q => q.Category.Trim().ToLower() == category.Trim().ToLower());
        }

        var dbQuestions = query.ToList();

        if (!dbQuestions.Any())
        {
            TempData["Error"] = "No questions found for this selection.";
            return RedirectToAction("Difficulty", new { lang = lang });
        }

        var randomizedQuestions = dbQuestions
            .OrderBy(x => Guid.NewGuid())
            .Take(questions)
            .Select(q => new Question
            {
                Id = q.Id,
                Language = q.Language,
                Difficulty = q.Difficulty,
                Category = q.Category,
                QuestionText = q.QuestionText,
                CorrectAnswer = q.CorrectAnswer,
                Explanation = q.Explanation,
                Choices = q.Choices.Select(c => c.ChoiceText).ToList()
            })
            .ToList();

        HttpContext.Session.SetObject("QuizQuestions", randomizedQuestions);
        HttpContext.Session.SetObject("QuizAnswers", new List<QuizAnswer>());

        return RedirectToAction("Question", new { index = 0 });
    }

    public IActionResult Question(int index)
    {
        var questions = HttpContext.Session.GetObject<List<Question>>("QuizQuestions");

        if (questions == null || index >= questions.Count)
        {
            return RedirectToAction("Results");
        }

        var model = new QuizSessionViewModel
        {
            Language = questions[index].Language,
            Difficulty = questions[index].Difficulty,
            CurrentQuestionIndex = index,
            TotalQuestions = questions.Count,
            CurrentQuestion = questions[index]
        };

        return View("Questions", model);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(int questionId, string userAnswer, int index)
    {
        var questions = HttpContext.Session.GetObject<List<Question>>("QuizQuestions");
        var answers = HttpContext.Session.GetObject<List<QuizAnswer>>("QuizAnswers");

        if (questions == null || answers == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var question = questions.First(q => q.Id == questionId);

        answers.Add(new QuizAnswer
        {
            QuestionId = questionId,
            UserAnswer = userAnswer,
            IsCorrect = userAnswer == question.CorrectAnswer
        });

        HttpContext.Session.SetObject("QuizAnswers", answers);

        if (index >= questions.Count - 1)
        {
            int correctCount = answers.Count(a => a.IsCorrect);
            int totalQuestions = questions.Count;

            string difficulty = questions.First().Difficulty;
            string language = questions.First().Language;

            double basePointsPerQuestion = 5.0;
            double rawScore = correctCount * basePointsPerQuestion;

            double multiplier = 1.0;
            if (string.Equals(difficulty, "Medium", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1.5;
            }
            else if (string.Equals(difficulty, "Hard", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 2.0;
            }

            double multipliedScore = rawScore * multiplier;

            double bonusPoints = 0.0;
            bool isPerfectScore = (correctCount == totalQuestions);

            if (isPerfectScore)
            {
                if (totalQuestions == 10)
                {
                    bonusPoints = 5.0;
                }
                else if (totalQuestions == 20)
                {
                    bonusPoints = 10.0;
                }
            }

            double finalScore = multipliedScore + bonusPoints;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var scoreRecord = new UserScoreEntity
                {
                    UserId = userId!,
                    Language = language,
                    Difficulty = difficulty,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctCount,
                    FinalScore = finalScore,
                    DateCompleted = DateTime.UtcNow
                };

                _context.UserScores.Add(scoreRecord);
                await _context.SaveChangesAsync();
            }

            // Notice: Session.Remove has been taken out of here entirely!

            // Pass the float cleanly through TempData using String Invariant Culture
            TempData["Score"] = finalScore.ToString(System.Globalization.CultureInfo.InvariantCulture);
            TempData["CorrectCount"] = correctCount;
            TempData["TotalQuestions"] = totalQuestions;
            TempData["Language"] = language;
            TempData["Difficulty"] = difficulty;
            TempData["EarnedBonus"] = (bonusPoints > 0);

            return RedirectToAction("Results");
        }

        return RedirectToAction("Question", new { index = index + 1 });
    }

    private double CalculateQuizScore(int correctCount, int totalQuestions, string difficulty)
    {
        double basePointsPerQuestion = 5.0;
        double rawScore = correctCount * basePointsPerQuestion;

        double multiplier = 1.0;
        if (string.Equals(difficulty, "Medium", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 1.5;
        }
        else if (string.Equals(difficulty, "Hard", StringComparison.OrdinalIgnoreCase))
        {
            multiplier = 2.0;
        }

        double multipliedScore = rawScore * multiplier;

        double bonusPoints = 0.0;
        bool isPerfectScore = (correctCount == totalQuestions);

        if (isPerfectScore)
        {
            if (totalQuestions == 10)
            {
                bonusPoints = 5.0;
            }
            else if (totalQuestions == 20)
            {
                bonusPoints = 10.0;
            }
        }

        return multipliedScore + bonusPoints;
    }

    public IActionResult Results()
    {
        var questions = HttpContext.Session.GetObject<List<Question>>("QuizQuestions");
        var answers = HttpContext.Session.GetObject<List<QuizAnswer>>("QuizAnswers");

        if (questions == null || !questions.Any() || answers == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var reviews = new List<QuestionReview>();
        int correctCount = 0;

        foreach (var answer in answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            if (answer.IsCorrect) correctCount++;

            reviews.Add(new QuestionReview
            {
                QuestionText = question.QuestionText,
                CorrectAnswer = question.CorrectAnswer,
                UserAnswer = answer.UserAnswer,
                Explanation = question.Explanation,
                IsCorrect = answer.IsCorrect
            });
        }

        string difficulty = questions.FirstOrDefault()?.Difficulty ?? "Easy";

        double exactPoints = 0.0;
        if (TempData["Score"] != null)
        {
            string scoreString = TempData["Score"].ToString()!;
            double.TryParse(scoreString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out exactPoints);

            // Keep the value in TempData in case they refresh the page right away
            TempData.Keep("Score");
        }
        else
        {
            exactPoints = CalculateQuizScore(correctCount, questions.Count, difficulty);
        }

        var model = new QuizResultViewModel
        {
            Language = questions.FirstOrDefault()?.Language ?? "Unknown",
            Difficulty = difficulty,
            Score = correctCount,
            FinalPoints = exactPoints,
            TotalQuestions = questions.Count,
            Reviews = reviews
        };

        // Notice: Session.Remove has been deleted from here too!
        // The data stays active so users can safely refresh or print the page.

        return View(model);
    }
}