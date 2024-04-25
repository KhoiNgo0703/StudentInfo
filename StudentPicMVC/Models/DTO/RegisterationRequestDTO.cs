using System.ComponentModel.DataAnnotations;

namespace StudentPicMVC.Models.DTO
{
    public class RegisterationRequestDTO
    {
        [Required]
        public string UserName { get; set; }
        public string Name { get; set; }

        [Required]        
        public string Password { get; set; }
        public string Role { get; set; }        
    }
}
