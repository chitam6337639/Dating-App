//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace CSDL.Models
//{
//    public class UserDislikedRelation
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int UserDislikeRelationID { get; set; }

//        // Khóa ngoại tham chiếu đến User
//        [ForeignKey("UserID")]
//        public int UserID { get; set; }
//        public virtual User User { get; set; }

//        // Khóa ngoại tham chiếu đến DislikedUser
//        [ForeignKey("DislikedUserID")]
//        public int DislikedUserID { get; set; }
//        public virtual DislikedUser DislikedUser { get; set; }
//    }
//}
