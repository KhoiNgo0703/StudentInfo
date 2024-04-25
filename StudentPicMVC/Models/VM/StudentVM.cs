using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Models.VM
{
    public class StudentVM
    {
        public StudentVM() 
        {
            Student = new StudentDTO();
            StudentList= new List<StudentDTO>();
        }
        public StudentDTO Student { get; set; }

        public List<StudentDTO> StudentList { get; set; }
    }
}
