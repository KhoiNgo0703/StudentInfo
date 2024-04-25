using Microsoft.AspNetCore.Identity;

namespace StudentPicAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
