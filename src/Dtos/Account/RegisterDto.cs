using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Password { get; set; }
    }
}
