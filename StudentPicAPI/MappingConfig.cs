using AutoMapper;
using StudentPicAPI.Models.DTO;
using StudentPicAPI.Models;

namespace StudentPicAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            //Student------------
            CreateMap<Student, StudentDTO>();
            CreateMap<StudentDTO, Student>();
            CreateMap<Student, StudentUpdateDTO>().ReverseMap();
            CreateMap<Student, StudentCreateDTO>().ReverseMap();

            //-----------User-----------
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
        }
    }
}
