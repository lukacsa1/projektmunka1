namespace webshop.DTOs
{
    public class RegistrationDTO
    {
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string TempHash { get; set; }
        public string Salt { get; set; }
    }
}
