using Newtonsoft.Json;
using Student_Utility;
using StudentPicMVC.Models;
using StudentPicMVC.Services.IServices;
using System.Net.Mime;
using System.Text;

namespace StudentPicMVC.Services
{
    public class ApiMessageRequestBuilder : IApiMessageRequestBuilder
    {
        public HttpRequestMessage Build(APIRequest apiRequest)
        {
            HttpRequestMessage message = new();
            //header
            message.Headers.Add("Accept", "application/json");

            //url
            message.RequestUri = new Uri(apiRequest.Url);

            //data Content validation
            if (apiRequest.Data != null)
            {
                //serialize the data
                message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json");
            }

            //method
            //define http type
            switch (apiRequest.ApiType)
            {
                case SD.ApiType.POST:
                    message.Method = HttpMethod.Post;
                    break;
                case SD.ApiType.PUT:
                    message.Method = HttpMethod.Put;
                    break;
                case SD.ApiType.DELETE:
                    message.Method = HttpMethod.Delete;
                    break;
                default:
                    message.Method = HttpMethod.Get;
                    break;
            }
            return message;
        }
    }
}
