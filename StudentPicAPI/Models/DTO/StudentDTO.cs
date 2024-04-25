using System.ComponentModel.DataAnnotations;

namespace StudentPicAPI.Models.DTO
{
    public class StudentDTO
    {
        public int Id { get; set; }

        //set require for Name
        public string Name { get; set; }
        [Range(10, 90, ErrorMessage = "Age must be between 10 and 90.")]
        public int Age { get; set; }
        public string Class { get; set; }
        public string ImageB64 { get; set; }

    }
}
