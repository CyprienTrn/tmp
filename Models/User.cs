using System.Text;
using System.Text.Json.Serialization;

namespace link_up.Models
{
    public class User
    {
        [JsonIgnore]
        public string id { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore]
        public string user_id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}