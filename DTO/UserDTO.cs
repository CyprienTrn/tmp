
namespace link_up.DTO
{

    public class UserDTO
    {
        public string id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
