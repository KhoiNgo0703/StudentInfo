using StudentPicMVC.Models.DTO;

namespace StudentPicMVC.Services.IServices
{
    public interface ITokenProvider
    {
        void SetToken(TokenDTO tokenDTO);
        TokenDTO? GetToken();
        void ClearToken();
    }
}
