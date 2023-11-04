using System.ComponentModel.DataAnnotations;

namespace CSDL.DTO
{
    public class UserInfo
    {
        [Key]
        public int userId { get; set; }
        public string gender { get; set; }
        public string? ImageURL { get; set; }
        public string? bio { get; set; }
        public DateTime? birthday { get; set; }
        public string? lastName { get; set; }
        public string? firstName { get; set; }
        public string? location { get; set; }
        public string? accessToken { get; set; }
    }
}
