namespace Code_Quizzer.Models
{
    // This is for your Quiz Logic and Session
    public class Question
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public string Difficulty { get; set; }
        public string QuestionText { get; set; }
        public List<string> Choices { get; set; } // List of strings for the UI
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public string Category { get; set; } // e.g., "OOP", Algorithms", "Syntax", etc.
    }

    // This is specifically for the SQL Database Table
    public class QuestionEntity
    {
        public int Id { get; set; }
        public string Language { get; set; }
        public string Difficulty { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public string Category { get; set; }
        // Navigation property for SQL relationship
        public List<QuestionChoice> Choices { get; set; }
    }

    public class QuestionChoice
    {
        public int Id { get; set; }
        public int QuestionEntityId { get; set; }
        public string ChoiceText { get; set; }
    }
}