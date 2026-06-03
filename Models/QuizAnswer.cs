namespace Code_Quizzer.Models
{
    public class QuizAnswer
    {
        public int QuestionId { get; set; }
        public string UserAnswer { get; set; } = "";
        public bool IsCorrect { get; set; }
    }
}
