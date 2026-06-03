namespace Code_Quizzer.Models
{
    public class QuizSessionViewModel
    {
        public string Language { get; set; } = "";
        public string Difficulty { get; set; } = "";
        public int CurrentQuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public Question CurrentQuestion { get; set; } = new();
    }
}
