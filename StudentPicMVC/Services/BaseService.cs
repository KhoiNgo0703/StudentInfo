using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Student_Utility;
using StudentPicMVC.Models;
using StudentPicMVC.Models.DTO;
using StudentPicMVC.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Net.Http;

namespace StudentPicMVC.Services
{
    public class BaseService : IBaseService
    {
        //implement the IBaseService----
        //
        //token provider
        private readonly ITokenProvider _tokenProvider;
        //http context
        private IHttpContextAccessor _httpContextAccessor;
        //call student url
        protected readonly string StudentURL;
        //message builder
        private readonly IApiMessageRequestBuilder _apiMessageRequestBuilder;
        public APIResponse responseModel { get; set; }

        //HttpClient- on same server
        public IHttpClientFactory httpClient { get; set; }

        //implement
        public BaseService(IHttpClientFactory httpClient, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor, IApiMessageRequestBuilder apiMessageRequestBuilder, IConfiguration configuration)
        {
            this.responseModel = new();
            //dependency injection for httpClient
            this.httpClient = httpClient;
            _tokenProvider = tokenProvider;
            _httpContextAccessor = httpContextAccessor;
            _apiMessageRequestBuilder = apiMessageRequestBuilder;
            StudentURL = configuration.GetValue<string>("ServiceUrls:StudentAPI");
        }
        public async Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true)
        {
            try
            {
                //create a client called CustomerAPI
                var client=httpClient.CreateClient("CustomerAPI");
                //-------------------------
                //create a message factory to store all the request messages
                var messageFactory = () =>
                {
                    return _apiMessageRequestBuilder.Build(apiRequest);
                };
                //response
                HttpResponseMessage httpResponseMessage = null;
                httpResponseMessage = await SendWithRefreshTokenAsync(client, messageFactory, withBearer);

                //final API Response
                APIResponse FinalApiResponse = new()
                {
                    IsSuccess = false
                };
                try
                {
                    switch (httpResponseMessage.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            FinalApiResponse.ErrorMessage = new List<string>() { "Not Found" };
                            break;
                        case HttpStatusCode.Forbidden:
                            FinalApiResponse.ErrorMessage = new List<string>() { "Access Denied" };
                            break;
                        case HttpStatusCode.Unauthorized:
                            FinalApiResponse.ErrorMessage = new List<string>() { "Unauthorized" };
                            break;
                        case HttpStatusCode.InternalServerError:
                            FinalApiResponse.ErrorMessage = new List<string>() { "Internal Server Error" };
                            break;
                        default:
                            //extract content
                            var apiContent = await httpResponseMessage.Content.ReadAsStringAsync();
                            FinalApiResponse.IsSuccess = true;
                            FinalApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                            break;
                    }
                }
                catch (Exception e)
                {
                    //using var and T form
                    FinalApiResponse.ErrorMessage = new List<string>() { "Error Encountered", e.Message.ToString() };
                }

                var res = JsonConvert.SerializeObject(FinalApiResponse);
                var returnObj = JsonConvert.DeserializeObject<T>(res);
                return returnObj;
            }
            catch (AuthException)
            {
                throw;
            }
            catch (Exception e)
            {
                var dto = new APIResponse
                {
                    ErrorMessage = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false,
                };
                //deserialize
                var res = JsonConvert.SerializeObject(dto);
                //serialize
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            }
        }

        //---------------------------------------------------------------
        private async Task<HttpResponseMessage> SendWithRefreshTokenAsync(HttpClient httpClient,
            Func<HttpRequestMessage> httpRequestMessageFactory, bool withBearer = true)
        {
            if (!withBearer)
            {
                //no token request-> normal send async
                return await httpClient.SendAsync(httpRequestMessageFactory());
            }
            else
            {
                //token request->get token
                TokenDTO tokenDTO = _tokenProvider.GetToken();
                //check token
                //if valid 
                if (tokenDTO != null && !string.IsNullOrEmpty(tokenDTO.AccessToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken);
                }

                try
                {
                    var response = await httpClient.SendAsync(httpRequestMessageFactory());
                    if (response.IsSuccessStatusCode)
                        return response;
                    // IF this fails then we can pass refresh token!
                    if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //GENERATE NEW Token from Refresh token / Sign in with that new token and then retry
                        await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        response = await httpClient.SendAsync(httpRequestMessageFactory());
                        return response;
                    }
                    return response;
                }
                catch (AuthException)
                {
                    throw;
                }
                catch (HttpRequestException httpRequestException)
                {
                    if (httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // refresh token and retry the request
                        await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        return await httpClient.SendAsync(httpRequestMessageFactory());
                    }
                    throw;
                }

            }
        }

        private async Task InvokeRefreshTokenEndpoint(HttpClient httpClient, string existingAccessToken, string existingRefreshToken)
        {
            //make a request
            HttpRequestMessage message = new();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri($"{StudentURL}/api/{SD.CurrentAPIVersion}/UsersAuth/refresh");
            message.Method = HttpMethod.Post;
            message.Content = new StringContent(JsonConvert.SerializeObject(new TokenDTO()
            {
                AccessToken = existingAccessToken,
                RefreshToken = existingRefreshToken
            }), Encoding.UTF8, "application/json");

            //get the response
            var response = await httpClient.SendAsync(message);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<APIResponse>(content);

            //if response is not valid
            if (apiResponse?.IsSuccess != true)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync();
                _tokenProvider.ClearToken();
                throw new AuthException();
            }
            else
            {
                //if response is valid
                var tokenDataStr = JsonConvert.SerializeObject(apiResponse.Result);
                var tokenDto = JsonConvert.DeserializeObject<TokenDTO>(tokenDataStr);

                if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.AccessToken))
                {
                    //New method to sign in with the new token that we receive                   
                    await SignInWithNewTokens(tokenDto);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
                }
            }


        }

        private async Task SignInWithNewTokens(TokenDTO tokenDTO)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenDTO.AccessToken);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name").Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));
            var principal = new ClaimsPrincipal(identity);
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _tokenProvider.SetToken(tokenDTO);
        }
    }
}
