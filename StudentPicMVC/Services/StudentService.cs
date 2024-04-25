using Student_Utility;
using StudentPicMVC.Models;
using StudentPicMVC.Models.DTO;
using StudentPicMVC.Services.IServices;

namespace StudentPicMVC.Services
{
    public class StudentService : IStudentService
    {
        //implement ihttpclientfactory
        private readonly IHttpClientFactory _clientFactory;
        //implement api url
        private string studentURL;
        //implement base service
        private readonly IBaseService _baseService;

        //remember to add path to the base class
        public StudentService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService)  
        {
            _clientFactory = clientFactory;
            studentURL = configuration.GetValue<string>("ServiceUrls:StudentAPI");
            _baseService = baseService;
        }
        public async Task<T> CreateAsync<T>(StudentCreateDTO dto)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType=SD.ApiType.POST,
                Data = dto,
                Url= studentURL+ $"/api/{SD.CurrentAPIVersion}/StudentAPI",

            });
        }

        public async Task<T> DeleteAsync<T>(int id)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.DELETE,                
                Url = studentURL + $"/api/{SD.CurrentAPIVersion}/StudentAPI/"+id,

            });
        }

        public async Task<T> GetAllAsync<T>()
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,                
                Url = studentURL + $"/api/{SD.CurrentAPIVersion}/StudentAPI",
            });
        }

        public async Task<T> GetAsync<T>(int id)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.GET,                
                Url = studentURL + $"/api/{SD.CurrentAPIVersion}/StudentAPI/"+id,

            });
        }

        public async Task<T> UpdateAsync<T>(StudentUpdateDTO dto)
        {
            return await _baseService.SendAsync<T>(new APIRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = studentURL + $"/api/{SD.CurrentAPIVersion}/StudentAPI/"+dto.Id,

            });
        }
    }
}
