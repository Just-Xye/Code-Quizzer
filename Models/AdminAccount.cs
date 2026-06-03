using System.ComponentModel.DataAnnotations;

namespace LostAndFound.Models
{
    public class AdminAccount
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}