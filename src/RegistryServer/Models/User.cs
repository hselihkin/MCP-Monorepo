using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryServer.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = new byte[0];

        [Required]
        public byte[] PasswordSalt { get; set; } = new byte[0];

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
    }
}
