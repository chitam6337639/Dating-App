using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CSDL.Models
{
    public class Message
    {
        [Key]
        public int messageId { get; set; }
        public string? content { get; set; }
        public DateTime? timeSent { get; set; }
        public string? status { get; set; }


        public int UserIdTo { get; set; } // This is the foreign key
        [ForeignKey("UserIdTo")]
        [JsonIgnore]
        public virtual User UserTo { get; set; } // This is the navigation property to User

        
        public int UserIdFrom { get; set; } // This is the foreign key
        [ForeignKey("UserIdFrom")]
        [JsonIgnore]
        public virtual User UserFrom { get; set; } 
    }
}
