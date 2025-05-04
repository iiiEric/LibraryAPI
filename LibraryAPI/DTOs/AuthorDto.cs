using LibraryAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class AuthorDTO
    {
        public int Id { get; set; }

        public required string FullName { get; set; }
    }
}
