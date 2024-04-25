using Student_Utility;
using StudentPicMVC.Models;
using StudentPicMVC.Models.DTO;
using StudentPicMVC.Services.IServices;

namespace StudentPicMVC.Services
{
    public class AuthService : IAuthService
    {
        //-----------------------------------implementation---------------------------------
        //implement ihttpclientfactory
        private readonly IHttpClientFactory _clientFactory;
        //implement api url
        private string studentURL;
        //implement base service
        private readonly IBaseService _baseService;

        public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService)
        {
            _clientFactory = clientFactory;
            _baseService = baseService;
            studentURL = configuration.GetValue<string>("ServiceUrls:StudentAPI");
        }
        
        //------------------------------------task-------------------------------------------
        public async Task<T> LoginAsync<T>(LoginRequestDTO objToCreate)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = objToCreate,
                Url = studentURL + "/api/UsersAuth/login"
            }, withBearer: false);
        }

        public async Task<T> RegisterAsync<T>(RegisterationRequestDTO objToCreate)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = objToCreate,
                Url = studentURL + "/api/UsersAuth/register"
            }, withBearer: false);
        }

        public async Task<T> LogoutAsync<T>(TokenDTO obj)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = obj,
                Url = studentURL + $"/api/{SD.CurrentAPIVersion}/UsersAuth/revoke"
            });
        }
    }
}
