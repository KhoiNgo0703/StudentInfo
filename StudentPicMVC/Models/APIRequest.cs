using System.Security.AccessControl;
using static Student_Utility.SD;

namespace StudentPicMVC.Models
{
    public class APIRequest
    {
        //things need to request 
        public ApiType ApiType { get; set; } = ApiType.GET;//set default=get
        public string Url { get; set; }
        public object Data { get; set; }
        public string Token { get; set; }
    }
}
