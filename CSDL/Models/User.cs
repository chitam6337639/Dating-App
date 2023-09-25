using System.ComponentModel.DataAnnotations;

namespace CSDL.Models
{
    public class User
    {
        [Key]
        public int userId { get; set; }
        public string gender { get; set; }
        public string? ImageURL { get; set; }
        public string? bio { get; set; }
        
        public DateTime? birthday { get; set; }

        public ICollection<History>? Histories { get; set; }
        public ICollection<Image>? Images { get; set; }
        public ICollection<Match>? Matches { get; set; }
        public ICollection<Message>? Messages { get; set; }
    }
}
