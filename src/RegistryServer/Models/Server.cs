using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace RegistryServer.Models
{
    [Table("Server")]
    public class Server
    {
        [Key]
        [Required]
        [StringLength(100)]
        public Uri? Endpoint { get; set; }

        [StringLength(100)]
        public string ServerName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ServerDesc { get; set; }

        // Store tools as JSON string in database
        public string ToolsJson { get; set; } = "[]";

        // Store arguments as JSON string in database
        public string ArgumentsJson { get; set; } = "{}";

        // Navigation properties (not mapped to database)
        [NotMapped]
        public List<string> Tools
        {
            get => string.IsNullOrEmpty(ToolsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(ToolsJson) ?? new List<string>();
            set => ToolsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Dictionary<string, Dictionary<string, string>> Arguments
        {
            get => string.IsNullOrEmpty(ArgumentsJson) ? new Dictionary<string, Dictionary<string, string>>() : JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(ArgumentsJson) ?? new Dictionary<string, Dictionary<string, string>>();
            set => ArgumentsJson = JsonSerializer.Serialize(value);
        }
    }
}
