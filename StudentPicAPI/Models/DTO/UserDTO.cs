namespace StudentPicAPI.Models.DTO
{
    public class UserDTO
    {
        public UserDTO()
        {
            ErrorDescription = new List<string>();
        }
        public string ID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public List<string> ErrorDescription { get; set; }
    }
}
