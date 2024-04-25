using StudentPicMVC.Models;

namespace StudentPicMVC.Services.IServices
{
    public interface IApiMessageRequestBuilder
    {
        HttpRequestMessage Build(APIRequest apiRequest);
    }
}
