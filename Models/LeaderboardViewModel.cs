namespace Code_Quizzer.Models
{
    public class LeaderboardViewModel
    {
        public int Rank { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public double TotalPoints { get; set; }
    }
}