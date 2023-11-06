namespace CSDL.DTO
{
    public class LatestMessageDTO
    {
        public int UserId { get; set; }
        public string LatestMessageContent { get; set; }
        public DateTime? LatestMessageTimeSent { get; set; }
    }
}
