using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CSDL.Models
{
    public class Image
    {
        [Key]
        public int imageId {  get; set; }
        public string? ImageURL { get; set; }
        public int userId { get; set; }
        [ForeignKey("userId")]
        [JsonIgnore]
        public User User { get; set; }
    }
}
