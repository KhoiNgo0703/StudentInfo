using StudentPicAPI.Data;
using StudentPicAPI.Models;
using StudentPicAPI.Repository.IRepository;

namespace StudentPicAPI.Repository
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        private readonly ApplicationDBContext _db;
        public StudentRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Student> UpdateAsync(Student entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _db.Students.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
