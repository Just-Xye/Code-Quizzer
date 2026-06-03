namespace Code_Quizzer.Models
{
    public class QuizResultViewModel
    {
        public string Language { get; set; } = "";
        public string Difficulty { get; set; } = "";
        public int Score { get; set;  }
        public double FinalPoints { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionReview> Reviews { get; set; } = new();
    }
}
