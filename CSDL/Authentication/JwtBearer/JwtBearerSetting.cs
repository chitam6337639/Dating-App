namespace CSDL.Authentication.JwtBearer
{
    public class JwtBearerSetting
    {
        public string Issuer { get; set; }
        public string Audience { get; set;}
        public string SigingKey { get; set;}
    }
}
