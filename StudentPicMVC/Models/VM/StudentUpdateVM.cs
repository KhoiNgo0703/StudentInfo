using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Models.VM
{
    public class StudentUpdateVM
    {
        public StudentUpdateVM() 
        {
            Student = new StudentUpdateDTO();
        }
        public StudentUpdateDTO Student {  get; set; }
    }
}
