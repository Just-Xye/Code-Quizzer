// Models/UserScoreEntity.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Code_Quizzer.Models
{
    public class UserScoreEntity
    {
        [Key]
        public int Id { get; set; }
        // Links the score directly to the logged-in Identity User
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        [Required]
        public string Language { get; set; }
        [Required]
        public string Difficulty { get; set; }
        [Required]
        public int TotalQuestions { get; set; }
        [Required]
        public int CorrectAnswers { get; set; }
        [Required]
        public double FinalScore { get; set; } // Keeps precision for 1.5x calculations
        public DateTime DateCompleted { get; set; } = DateTime.UtcNow;
    }
}