using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSDL.Models
{
    public class Account
    {
        [Key]
        public int accountId { get; set; }
        public string email { get; set; } = string.Empty;
        public byte[] passwordHash { get; set; } = new byte[32];
        public byte[] passwordSalt{ get; set; } = new byte[32];
        public string? verificationToken { get; set; }  
        public DateTime? verifiedAt { get; set; }   
        public string? passwordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }  
        public string status { get; set; }

    }
}
