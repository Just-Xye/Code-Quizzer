namespace Code_Quizzer.Models
{
    public class QuestionReview
    {
        public string QuestionText { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public string UserAnswer { get; set; } = "";
        public string Explanation { get; set; } = "";
        public bool IsCorrect { get; set; }
    }
}
