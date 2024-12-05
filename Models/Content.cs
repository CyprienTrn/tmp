using System.Text.Json.Serialization;

namespace link_up.Models
{
    public class Content
    {
        [JsonIgnore]
        public string id { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore]
        public string content_id { get; set; }
        public List<Media> medias { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
    }
}