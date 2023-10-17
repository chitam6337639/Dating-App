namespace CSDL.DTO
{
    public class CreateUser
    {
        public int accountId { get; set; }
        public string? lastName { get; set; }
        public string? firstName { get; set; }
        public string gender { get; set; }
        public DateTime? birthday { get; set; }
        public string? location { get; set; }
    }
}
