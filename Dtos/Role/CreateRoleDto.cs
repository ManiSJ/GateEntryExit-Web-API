﻿using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Dtos.Role
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Role Name is required.")]
        public string RoleName { get; set; } = null!;
    }
}
