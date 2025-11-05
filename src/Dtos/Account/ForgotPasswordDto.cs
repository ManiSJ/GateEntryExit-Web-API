using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Dtos.Account
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } 
    }
}
