using StudentPicAPI.Models;

namespace StudentPicAPI.Repository.IRepository
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student> UpdateAsync(Student entity);

    }
}
