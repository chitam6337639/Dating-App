namespace CSDL.DTO
{
    public class MatchedUserWithLatestMessageDTO
    {
        public int UserId { get; set; }
        public string LatestMessageContent { get; set; }
        public DateTime? LatestMessageTimeSent { get; set; }
        public int? LatestMessageUserIdFrom { get; set; }
        public string Gender { get; set; }
        public string ImageURL { get; set; }
        public string Bio { get; set; }
        public DateTime? Birthday { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Location { get; set; }
        public string AccessToken { get; set; }
    }
}
