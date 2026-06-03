public class DifficultyViewModel
{
    public string Language { get; set; } = "";
    public List<string> Difficulties { get; set; } = new();
    public List<int> QuestionCounts { get; set; } = new();
    public List<string> Categories { get; set; } = new List<string>();
}