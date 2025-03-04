namespace webshop.DTOs
{
    public class RecoverPasswordDTO
    {
        public string loginName { get; set; }
        public string email { get; set; }
        public string authCode { get; set; }
        public string tmpHash { get; set; }
    }
}
