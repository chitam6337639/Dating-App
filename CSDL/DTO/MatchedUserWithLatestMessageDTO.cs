namespace CSDL.DTO
{
    public class MatchedUserWithLatestMessageDTO
    {
        public int userId { get; set; }
        public string latestMessageContent { get; set; }
        public DateTime? latestMessageTimeSent { get; set; }
        public int? latestMessageUserIdFrom { get; set; }
        public string gender { get; set; }
        public string imageURL { get; set; }
        public string bio { get; set; }
        public DateTime? birthday { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string location { get; set; }
        public string accessToken { get; set; }
    }
}
