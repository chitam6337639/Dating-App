//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace CSDL.Models
//{
//    public class UserLikeRelation
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int UserLikeRelationID { get; set; }

//        // Khóa ngoại tham chiếu đến User
//        [ForeignKey("UserID")]
//        public int UserID { get; set; }
//        public virtual User User { get; set; }

//        // Khóa ngoại tham chiếu đến LikeUser
//        [ForeignKey("LikeUserID")]
//        public int LikeUserID { get; set; }
//        public virtual LikeUser LikeUser { get; set; }
//    }
//}
