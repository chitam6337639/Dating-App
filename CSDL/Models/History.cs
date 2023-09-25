using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CSDL.Models
{
    public class History
    {
        [Key]
        public int activityId { get; set; }
        public DateTime? time { get; set; }
        public string? type { get; set; }

        public int UserId { get; set; } // This is the foreign key
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; } // This is the navigation property to User

        public int MatchId { get; set; } // This is the foreign key to Match, if applicable
        [ForeignKey("MatchId")]
        [JsonIgnore]
        public Match Match { get; set; }

    }
}
