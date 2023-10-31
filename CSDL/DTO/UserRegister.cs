using System.ComponentModel.DataAnnotations;

namespace CSDL.DTO
{
    public class UserRegister
    {
        //public int accountId { get; set; }
        [Required, EmailAddress]
        public string email { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters, dude!")]
        public string password { get; set; } = string.Empty;
        [Required, Compare("password")]
        public string confirmPassword { get; set; } = string.Empty;
    }
}
