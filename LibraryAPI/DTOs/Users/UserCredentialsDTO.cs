﻿using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs.Users
{
    public class UserCredentialsDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public string? Password { get; set; } //optional in order to instantiate
    }
}
