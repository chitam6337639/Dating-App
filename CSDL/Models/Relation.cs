using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CSDL.Models
{
    public class Relation
    {
        [Key]
        public int relationId {  get; set; }
        [ForeignKey("UserID")]
        public int UserID { get; set; }
        public virtual User User { get; set; }
        [ForeignKey("OtherUserId")]
        public int OtherUserId { get; set; }
        public virtual User OtherUser { get; set; }

        public bool isLike { get; set; }

        public bool isMatch { get; set; }
    }
}
