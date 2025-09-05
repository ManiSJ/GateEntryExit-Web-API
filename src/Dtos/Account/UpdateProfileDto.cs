using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Dtos.Account
{
    public class UpdateProfileDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }
    }
}
