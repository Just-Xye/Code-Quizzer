namespace Code_Quizzer.Models
{
    public class ProfileViewModel
    {
        public string ProfilePictureUrl { get; set; } = "";
        public string Bio { get; set; } = "";
        public int TotalQuizzes { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double TotalPoints { get; set; }
    }
}