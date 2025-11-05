using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Dtos.Account
{
    public class ChangePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        public string CurrentPassword { get; set; } 

        [Required]
        public string NewPassword { get; set; }
    }
}
