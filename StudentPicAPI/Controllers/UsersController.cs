using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentPicAPI.Models;
using StudentPicAPI.Models.DTO;
using StudentPicAPI.Repository.IRepository;
using System.Net;

namespace StudentPicAPI.Controllers
{
    //create the route
    [Route("api/UsersAuth")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        //------------------------------------------------Implementation---------------------------------
        //implement IUserRepository
        private readonly IUserRepository _userRepo;
        //declare APIResponse model
        protected APIResponse _response;
        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _response = new();
        }
        //-------------------------------------------------HTTP verb-------------------------------------
        //-----------------------Login POST------------------------
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            //get the token
            var tokenDto = await _userRepo.Login(model);
            //check user
            //invalid
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("User or password is incorrect");
                return BadRequest(_response);
            };
            //valid
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tokenDto;
            return Ok(_response);
        }
        //-----------------------Register POST---------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            //check username
            bool ifUserNameUnique = _userRepo.IsUniqueUser(model.UserName);
            if (!ifUserNameUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("UserName exists");
                return BadRequest(_response);
            }

             UserDTO user = await _userRepo.Register(model);
            if (user == null || user.ID == null|| (user.ErrorDescription != null && user.ErrorDescription.Count > 0))
            {
                _response.ErrorMessage.Add("Error while registering");
                if (user.ErrorDescription != null && user.ErrorDescription.Count > 0)
                {
                    _response.ErrorMessage.AddRange(user.ErrorDescription);
                }
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;                
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        //-----------------------RETURN NEW TOKEN FROM FRESH TOKEN POST---------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _userRepo.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage.Add("Token Invalid");
                    return Ok(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = tokenDTOResponse;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.Result = "Invalid input";
                return BadRequest(_response);
            }
        }
        //-----------------------MARK THE REFRESH TOKEN AS INVALID---------------------
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {

            if (ModelState.IsValid)
            {
                await _userRepo.RevokeRefreshToken(tokenDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            _response.IsSuccess = false;
            _response.Result = "Invalid Input";
            return BadRequest(_response);
        }
    }
}
