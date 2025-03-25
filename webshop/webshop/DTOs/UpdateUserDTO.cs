namespace webshop.DTOs
{
    public class UpdateUserDTO
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? LoginName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int? Active { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? PermissionLevel { get; set; }
    }
}
