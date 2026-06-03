namespace Code_Quizzer.Models
{
    public class QuestionUploadDTO
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public string Difficulty { get; set; }
        public string Category { get; set; }
        public string QuestionText { get; set; }
        public string ChoiceA { get; set; } // Matches Excel Column
        public string ChoiceB { get; set; } // Matches Excel Column
        public string ChoiceC { get; set; } // Matches Excel Column
        public string ChoiceD { get; set; } // Matches Excel Column
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }
}
