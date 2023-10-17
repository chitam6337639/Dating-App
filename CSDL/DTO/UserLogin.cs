using System.ComponentModel.DataAnnotations;

namespace CSDL.DTO
{
    public class UserLogin
    {
        [Required, EmailAddress]
        public string email { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
    }
}
