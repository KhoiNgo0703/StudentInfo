using AutoMapper;
using StudentPicMVC.Models.DTO;

namespace StudentPicMVC
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            //Student------------                
            CreateMap<StudentDTO, StudentUpdateDTO>().ReverseMap();
            CreateMap<StudentDTO, StudentCreateDTO>().ReverseMap();
        }
    }
}
