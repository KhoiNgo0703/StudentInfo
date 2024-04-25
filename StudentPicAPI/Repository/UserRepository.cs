using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentPicAPI.Data;
using StudentPicAPI.Models;
using StudentPicAPI.Models.DTO;
using StudentPicAPI.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentPicAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        //implement DBContext
        private readonly ApplicationDBContext _db;
        //implement UserManager for Identity
        private readonly UserManager<ApplicationUser> _userManager;
        //implement RoleManager for Identity
        private readonly RoleManager<IdentityRole> _roleManager;

        //implement automapper
        private readonly IMapper _mapper;

        //implement secret key
        private string secretKey;

        public UserRepository(ApplicationDBContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }
        //-------------------------------------------------------MAIN METHODS-------------------------------------------
        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            //check user name
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            //check password
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            //return empty access token if it is an invalid login
            if (user == null || isValid == false)
            {
                //return a response
                return new TokenDTO()
                {
                    AccessToken = "",
                };
            }

            //-----------------------------
            //token
            //create jwt token id
            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user, jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            //---------------------------------
            //get roles from database identity
            var roles = await _userManager.GetRolesAsync(user);

            //---------------------------------
            //return response
            TokenDTO tokenDto = new TokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
            return tokenDto;
        }

        public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            //retrieve info of a new user
            ApplicationUser user = new()
            {
                UserName = registerationRequestDTO.UserName,
                Email = registerationRequestDTO.UserName,
                NormalizedEmail = registerationRequestDTO.UserName.ToUpper(),
                Name = registerationRequestDTO.Name,
            };

            UserDTO userDTO = new();

            try
            {
                //create user 
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
                if (result.Errors != null && result.Errors.Any())
                {
                    // Loop through the errors and add them 
                    foreach (var error in result.Errors)
                    {
                        userDTO.ErrorDescription.Add(error.Description);
                    }
                }
                if (result.Succeeded)
                {
                    //only happen once
                    if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                    var userToReturn = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch (Exception ex)
            {

            }

            return userDTO;
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            // Find an existing refresh token
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            // Compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud
            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // When someone tries to use not valid refresh token, fraud possible
            if (!existingRefreshToken.IsValid)
            {
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            }
            // If just expired then mark as invalid and return empty
            if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            // replace old refresh with a new one with updated expire date
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            // revoke existing refresh token
            await MarkTokenAsInvalid(existingRefreshToken);

            // generate new access token
            var applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
                return new TokenDTO();
            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);
            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }
        //------------------------------------------------------------------------------------------------------------

        //---------------------------------SUPPORT METHODS------------------------------------------------------------
        //--------for Login--------------------
        public async Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId)
        {
            //check the role of user
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();

            //encode secretkey			
            var key = Encoding.ASCII.GetBytes(secretKey);

            //token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
					//new Claim(ClaimTypes.Name, user.Id.ToString()),
					new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()), //if have more than 1 role, need to loop it and add each of them
					new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                }),
                Expires = DateTime.UtcNow.AddMinutes(3),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            //generate token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenStr = tokenHandler.WriteToken(token);
            return tokenStr;

        }

        //create new refresh token
        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            //create new refreshToken with data
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(1),
                Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
            };
            //store it in the database
            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Refresh_Token;
        }

        //----------------------------------------

        //------------for refresh access token-------
        //read access token to return user id and token id
        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId == expectedUserId && jwtTokenId == expectedTokenId;

            }
            catch
            {
                return false;
            }
        }

        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            await _db.RefreshTokens.Where(u => u.UserId == userId
               && u.JwtTokenId == tokenId)
                   .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));

        }

        private Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            return _db.SaveChangesAsync();
        }
        //------------------for log out----------------------
        public async Task RevokeRefreshToken(TokenDTO tokenDTO)
        {
            //retrieve the refresh token from the database
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(_ => _.Refresh_Token == tokenDTO.RefreshToken);

            if (existingRefreshToken == null)
                return;

            // Compare data from existing refresh and access token provided and
            // if there is any missmatch then we should do nothing with refresh token

            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {

                return;
            }

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

        }
    }
}
