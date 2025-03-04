namespace webshop.DTOs
{
    public class LoggedUser
    {
        public string LoginName { get; set; }
        public string Email { get; set; }
        public int PermissionLevel { get; set; }
        public string Token { get; set; }
    }
}
