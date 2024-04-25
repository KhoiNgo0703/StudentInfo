using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Services.IServices
{
    public interface IStudentService
    {
        //perform all the CRUD functions
        Task<T> GetAsync<T>(int id);
        Task<T> GetAllAsync<T>();
        Task<T> CreateAsync<T>(StudentCreateDTO dto);
        Task<T> UpdateAsync<T>(StudentUpdateDTO dto);
        Task<T> DeleteAsync<T>(int id);
    }
}
