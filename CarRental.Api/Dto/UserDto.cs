using System.ComponentModel.DataAnnotations;

namespace CarRental.Api.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
