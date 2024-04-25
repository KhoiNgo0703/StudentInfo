using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Models.VM
{
   
    public class StudentDeleteVM
    {
        public StudentDeleteVM()
        {
            Student = new StudentDTO();
        }
        public StudentDTO Student { get; set; }
    }
}
