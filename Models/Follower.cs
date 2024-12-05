using System.Text.Json.Serialization;

namespace link_up.Models
{
    public class Follower
    {
        [JsonPropertyName("id")] // Mappe "Id" au format attendu "id"
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string FollowerUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}