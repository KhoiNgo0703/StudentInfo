using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        Task<T> RegisterAsync<T>(RegisterationRequestDTO objToCreate);
        Task<T> LogoutAsync<T>(TokenDTO obj);
    }
}
