using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Models.VM
{
    public class StudentCreateVM
    {
        public StudentCreateVM() 
        {
            Student =new StudentCreateDTO();
        }
        public StudentCreateDTO Student {  get; set; }
    }
}
