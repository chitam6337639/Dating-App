using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CSDL.Models
{
    public class Match
    {
        [Key]
        public int matchId { get; set; } 
        public DateTime? time { get; set; }


        public int UserId { get; set; } // This is the foreign key
        [JsonIgnore]
        public User User { get; set; } // This is the navigation property to User

        public int TargetUserId { get; set; }
        [JsonIgnore]// This is the foreign key to the target user
        public User TargetUser { get; set; }

    }
}
