using CSDL.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CSDL.AppModel
{
    public class Account
    {
        [Key]
        public string accountId { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string? accessToken { get; set; }
        [ForeignKey("userId")]
        [JsonIgnore]
        public User User { get; set; }
    }
}
