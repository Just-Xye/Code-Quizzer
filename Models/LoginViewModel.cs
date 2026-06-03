using System.ComponentModel.DataAnnotations;

namespace Code_Quizzer.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your username or email address.")]
        [Display(Name = "Username or Email")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}