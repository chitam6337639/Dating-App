using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string? lastName { get; set; }
        public string? firstName { get; set; }
        public string? location { get; set; }

        public string? accessToken { get; set; }

        public ICollection<History>? Histories { get; set; }
        public ICollection<Image>? Images { get; set; }
        public ICollection<Match>? Matches { get; set; }
        public ICollection<Message> UserMessages { get; set; }
        public ICollection<Message> OtherUserMessages { get; set; }
        public ICollection<Relation> UserRelations { get; set; }

        public ICollection<Relation> OtherUserRelations { get; set; }


        public int? accountId { get; set; }
        [ForeignKey("accountId")]
        [JsonIgnore]
        public Account Account { get; set; }
        //public ICollection<UserLikeRelation> UserLikeRelations { get; set; }
        //public ICollection<UserDislikedRelation> UserDislikedRelations { get; set; }
    }
}
