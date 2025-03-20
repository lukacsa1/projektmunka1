namespace webshop.DTOs
{
    public class ChangePasswordDTO
    {
        public string OldPasswordHash { get; set; }
        public string NewPasswordHash { get; set; }
        public string NewSalt { get; set; }
    }
}
