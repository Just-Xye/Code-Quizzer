using Microsoft.AspNetCore.Identity;

namespace Code_Quizzer.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Custom application fields
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
    }
}