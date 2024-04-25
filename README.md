RESTFUL API .NET 8 CORE: STUDENT MANAGEMENT PROJECT
---------------------------------------------STUDENT INFO-------------------------------------------------
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
---------------MODEL----------------------
1.Create Models and Dto for Student Management 
+Create Student Model and Dto models for it
+create Model for APIResponse
-------------------DATABASE--------------------------
1.Entity framework
+entityframeworkcore.sqlserver
+entityframeworkcore.tools
2.Database connection
+in appsetings.json,     "ConnectionStrings": { "DefaultSQLConnection": "Server=LAPTOP-83RRLTU3\\SQLEXPRESS;Database=Villa_API;user=sa;password=123456;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true" }
26.database dependency injection in program.cs
+builder.Services.AddDbContext<ApplicationDbContext>(option =>
{    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});
3.DBContext
+in model class Student.cs, bind the id as the key and use [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
+create class called ApplicationDbContext in Data folder
+include DbContext to inherit it from Entity Framework
+create DbSet
+seed data
4. Migrations
+PM console to create datatable:PM> add-migration AddVillaTable
+update migration : PM> update-database
+seed in PM console >add-migration SeedVillaTable
------how to add a new column to an existing database--------
+Create a new migration in PM>add-migration AddCustomNoteColumn
+in the migration file, modify the code:
    public partial class test1 : Migration
{
    /// <inheritdoc />
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE Students ADD CustomNote NVARCHAR(MAX) DEFAULT '123123'; ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE Students DROP COLUMN CustomNote;");
    }
}


public partial class Test2 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "PhoneNumber",
        table: "Students",
        nullable: false,
        defaultValue: new string("123"));

}

/// <inheritdoc />
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
      name: "PhoneNumber",
      table: "Students");

}
}
------------------Repository-----------------
+Create IRepository and Repository folder
+Create IStudentRepository and StudentRepository
++implement dbcontext in Repository and inherit interface in Repository
++recomend using T generic type to avoid when working with different DTOs
++implement repository in program.cs
----------------AutoMapper----------------
Setup AutoMapper and Mapping Config
+Used to avoid long code when inserting props for obj
+install AutoMapper and AutoMapperDependency 
+create a config file into project by creating a class called MappingConfig
+register in program.cs : builder.Services.AddAutoMapper(typeof(filename))
+use dependency injection to integrate automapper in controller
+modify the code in controller to integrate automapping
+for the generic repository, can not add it directly to program.cs but through other repositories
---------------Controller------------------
1.Create Controller for Student (StudentAPIController)
1.1.define route for controller
+use [Route("api/controllerName")]
+use [ApiController]
+need to define http verb for the action: add [HTTPGet]
+create action methods
------------------------------------------CONSUME API (MVC)---------------------------
///////////////////////////////////////////////////////////////////////////////////////////////
1.Set Up MVC project in the solution
+In Web proj
+change port number in both projects /Properties/launchSettings to differentiate projects
+set up to run multiple projects
------------------------------------Models---------------
Create DTOs and Models
+In Web proj
+create DTOs models for Student (do not need to set Key for props since MVC is not relevant to Database)
+create models for APIRequest and APIResponse
--------------------------------SD library-------------------------
Create a library Utility/SD to include httpverb and string token (using later for authentication)
+add the library to project reference to use SD file for ApiType in APIRequest Model
--------------------------------AutoMapper---------------------
Set Up AutoMapper for MVC
+In Web proj
+Install AutoMapper and AutoMapper Dependency injection
+create MappingConfig.cs and configure automapper in program.cs
----------------------------Service-----------------------------------
1.Link API proj to Web Proj
+Add API proj URL in web proj
++ Add "ServiceUrls": {
  "StudentAPI": "https://localhost:7211"
} to appsettings.json of web proj 
2.Base Services
+Create Services and IServices folder 
+Create BaseService class and IBaseService interface
++Purpose of this one is for sending APIRequest and receive the response 
++In IBaseService, create an action method to SendAsync and receive response
++in BaseService, create a client using HttpClient to request api
++create a message to send the request including these props:  Content, Header, Method, Url
++create a response using HttpResponseMessage apiResponse
++after getting apiResponse, checking the content then convert it to type APIResponse 
3.Student Services
+Create StudentService class and IStudentService interface
+Create action methods to perform CRUD functions
+In StudentService, inherit BaseService and IStudentService
+implement IhttpClientFactory and get api from ServiceUrls url by using IConfiguration
+modify the action methods in StudentService
+implement StudentService in program.cs
------------------------------Controller-------------------------------
1.Create StudentController
2.Implement IStudentService and automapper
3.Create Views for Student View, Student Update View, Student Delete View, Student Create View
+For View, it should have Get and Post, async for action methods and modelVM.
+Add SweetAlert2 for notification in layout.cshtml
+To fix issue table does not display after creating, 
+disable Nullable in project edit 
+alsoe in PM>add-migration ChangeNullableToFalse
-----------------------------STUDENT LOGIN+RESIGSTER-------------------------------------------------
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
---------------------------------------API-----------------------------------------------------------------
-----------------------------Model------------------------------------
1. Create models
+In the API proj, create models: UserDTO, LoginRequestDTO, RegisterationRequestDTO, TokenDTO.
++LoginRequestDTO: need username and password to login
++RegisterationRequestDTO:  need username, name, password, and role to register
++TokenDTO: need AccessToken to retrieve info of user
-----------------------------DataBase: .NET IDENTITY----------------------------------------------------
1.Configure .NET Identity
+in program.cs,configure AddIdentity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>();
+install the package 
+install AspNetCore.Identity.EntityFrameworkcore for api proj
2.create a model called ApplicationUser that inherits from IdentityUser
3.implement ApplicationUser in DBContext
+in ApplicationDbContext, inherit IdentityDbContext instead of DbContext.Ex: IdentityDbContext<ApplicationUser>
+add a navigation prop : public DbSet<ApplicationUser> ApplicationUsers { get; set; }
+try to make a new migration with add-migration AddIdentityUserTable but error shows up:
The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless 
++need to use base.OnModelCreating(modelBuilder); to override the default
++add the migration again and update database
-----------------------------Repository--------------------------------------------------------------------
1.Create UserRepository and IUserRepository
+IUserRepository: need 3 action methods: 
++IsUniqueUser: check the username
++Login: Take LoginRequestDTO and return TokenDTO to display and return token
++Register: Take RegisterationRequestDTO and return UserDTO to display
2.Add Api settings in appsettings.json
+"ApiSettings": {
  "Secret": "THIS IS USED TO SIGN AND VERIFY JWT TOKENS, REPLACE IT WITH YOUR OWN SECRET"
}
3.UserRepository
3.1.Implement DBContext, UserManager, RoleManager, automapper and secretkey 
3.2.Action Methods
+Create a method to create accessToken called GetAccessToken with parameter=jwtTokenId and user info from ApplicationUser
+In Login Method, check username, password, get the token, role and return tokenDto
+In Register Method, retrieve info of user, create a new user and add role if it has not existed, then return user info
3.3.Register Repository in Program.cs
+builder.Services.AddScoped<IUserRepository, UserRepository>();
3.Mapping UserDTO and ApplicationUser
------------------------------Controller-------------------------------------------------------------------
1.Create UsersController for API
2.Implement IUserRepo, APIResponse
3.Create http verb action methods: login(post), register(post)
3.1. Login
+Paramter = LoginRequestDTO
+get the token->check user validation->return api response
3.2.Register
+Parameter=RegisterationRequestDTO
+check the user name to see if it has existed in the database
+register a new user in the database
+return api response
--------------------------------------SECURE FOR API END POINT---------------------------------------
1.In Controller, to control the HTTP verb, add [Authorize(Roles="name of role"].
2.Authentication
+pass the token in Bearer
++Add app.UseAuthentication() before app.UseAuthorization() in program.cs
+in program.cs, configure the setting 
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,//evaluate the expire time of token 
        };
    });
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});
--------------------------------------------------------------------------------------------------------
----------------------------------WEB MVC FOR LOGIN+REGISTER----------------------------------
----------------------------------------Model---------------------------------------------------------
1.Create ModelDTO
+Copy and Paste models from API to Web proj
+In APIRequest, need to add Token for request
----------------------------------------Service---------------------------------------------------------
1.Modify Base Service
+We want to inject Base Service in other Service through dependency injection and remove base(clientFactory)
+remove inherit base service from other services
+use async and await + call SendAsync from _baseService to modify Services
+In IBaseService, In SendAsync method, add one more parameter=bool withBearer = true so that we do not need token for api request for unnecessary action methods later on.
///////////////////////////////////AuthService Service
2.Create AuthService and IAuthService
2.1.IAuthService
+Create 2 action methods:
++LoginAsync: with the parameter= LoginRequestDTO
++RegisterAsync:with the parameter= RegisterationRequestDTO
2.2 AuthService
+Implement IHttpClientFactory, api url, base service
+Login and Register action method: parameter=LoginRequestDTO/RegisterationRequestDTO to recieve API response through SendAsync withe parameter=new APIRequest()
+we can set withBearer: false for these 2 methods since later on, we do not pass token for the request
3.Implement AuthService in program.cs (AddScoped+AddHttpClient)
///////////////////////////////////TokenProvider Service
4.Create TokenProvider and ITokenProvider
4.1.ITokenProvider
+Create 3 methods: 
++void SetToken(TokenDTO tokenDTO): take data from tokenDTO and set the token to the cookie
++TokenDTO? GetToken(): get token from the cookie
void ClearToken(): clear the token
4.2TokenProvider
+Implement IHttpContextAccessor
+In SD.cs, rename SessionToken to AccessToken
+Register TokenProvider and HttpContextAccessor in program.cs
+Register for authentication in program.cs
++builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.Cookie.HttpOnly = true;
                  options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                  options.LoginPath = "/Auth/Login";
                  options.AccessDeniedPath = "/Auth/AccessDenied";
                  options.SlidingExpiration = true;
              });
++app.UseAuthentication() before app.UseAuthorization();
+To store and preserve token:
++Add app.UseSession(); after pipeline for preserving token purpose
++builder.Services.AddDistributedMemoryCache();
++builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
5.Modify BaseService to work with Token
+Implement ITokenProvider, IHttpContextAccessor
+When client send message request, check the authorization by getting token and pass it to new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken):
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken);
+send .SendAsync(message) to receive the response:  apiResponse = await client.SendAsync(message);
------------------------------------------Controller--------------------------------------------------------
1.Create AuthController
+Implement IAuthService, ITokenProvider
2. Create login, logout, register and access denied action methods
3.Log in
+In Login Post method:
++Retrieve APIResponse from LoginAsync
++Get the token from the result of APIResponse
++Read the token values-> tell the httpContext that user is logged In->get role and username from token->sign in with httpContext->SetToken in cookie->return to home page
3.1.Modify navbar for Login
+Use User.Identity.IsAuthenticated to toggle the navbar.
4.Register
+In SD.cs, create 2 const string for roles when registering: Admin and Customer
+In GET VIEW: populate the role of list from SD.cs and return the View
-------------------------------------------V2-Configure----------------------------------------------------
+Apply when you have different versions for api controller
1.SD.cs
+In SD.cs, add a new string to tell the CurrentAPIVersion
2.Controller for API
+Add //API version [ApiVersion("2.0")]
+Modify the route with the CurrentAPIVersion: [Route("api/v{version:apiVersion}/StudentAPI")]
+Modify builder.Services.AddSwaggerGen and app.UseSwaggerUI
+Add builder.Services.AddApiVersioning and builder.Services.AddVersionedApiExplorer
3.Services
+Update the URL in Services
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
---------------------------------------REFRESH TOKEN--------------------------------------------------
+How to use it
//When we log in, user will be given an access token and refreshtoken
//when user makes a request, if access token is expired, user will be given a new access token and refresh token to make a request
//if the refresh token expires, user needs to log in again
+refresh token will last longer than access token
+when login, server will give access +refreshtoken
+when access token expires, client need access token+refreshtoken to get the request done
+when both expires, need to login again
+need to make sure refresh token is only valid for 1 use
//-------------------------------------API-----------------------------------------------------------------
-------------------Model--------------------------------
1.create model called RefreshToken with Id=unique key
2.add public DbSet<RefreshToken> RefreshTokens {get;set;} in ApplicationDbContext to create the table
+add migration and update database
3.TokenDTO modification
+Add new prop called RefreshToken 
-------------------Repository----------------------------
1.Create RefreshAccessToken in IUserRepository
2.RefreshAccessToken in UserRepo
+Function:
// Find an existing refresh token
 // Compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud
// When someone tries to use not valid refresh token, fraud possible
// If just expired then mark as invalid and return empty
// replace old refresh with a new one with updated expire date
// revoke existing refresh token
// generate new access token
+We will handle this later
3.Create private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId) in UserRepo to return userId and tokenId to generate new refresh token
4.double check in login method in User Repo to see if it can generate jwtTokenId
5.Create private async Task<string> CreateNewRefreshToken(string userId, string tokenId) to create a new refresh token and save it to the database
6.Modify Login method in User Repo
+When user loggins, generate access token and refresh token and return it 
7.Work on RefreshAccessToken in UserRepo
----------------------Controller-------------------------------------
1.Create another API endpoint called refresh in UsersController
2.Create an action method called GetNewTokenFromRefreshToken
+It will return new refresh token and access token
+if it is valid, return good resposne: badrequest
///////////////////////////////////////////////////////////////////////////////////////////
--------------------------------------------MVC----------------------------------------
1.SD.cs: add new string called RefreshToken
2.TokenDTO: add new prop called RefreshToken
3.TokenProvider: modify to include RefreshToken
------------------Base Service--------------------------
1.In Base Service, use a messagefactory instead of a http request message since it is forbidden to use same message object more than 1
2.Add new service and iservice called IApiMessageRequestBuilder
3.Implement it in program.cs and use addsingleton since we do need a different one
+implement in baseservice
3.Modify SendAsync
+In SendAsync method, instead of using only httpClient.SendAsync(httpRequestMessageFactory()), we will create a method called SendWithRefreshTokenAsync since it can authorize user and also give user new refresh token and access token if they are expired
++Inside this method we will need a method called private async Task InvokeRefreshTokenEndpoint(HttpClient httpClient, string existingAccessToken, string existingRefreshToken) to generate a new access and refresh token and try to log in with these new tokens.
+++Implement APIURL
+Modify APIResponse 
--------------Custom Auth Exception--------
+Create another Service called AuthException and inherit from Exception
+Throw it in BaseService
+Need to make the extension so when the auth exception triggers, it will redirect to homepage
+In Web proj, create a folder called Extensions, inside create a class called AuthExceptionRedirection
+this class will inherit from IExceptionFilter
+Implement the filter in program.cs of web proj: inside AddcontrollersWithViews
-------------Revoke token on log out------------
+when user logs out, we want to mark the refresh token as invalid
+add Task RevokeRefreshToken(TokenDTO tokenDTO); in UserRepo
+in UsersController, add action method called revoke using http post
+in AuthService , add logoutasync method and modify AuthController
+Create another Service called AuthException and inherit from Exception
+Throw it in BaseService
+Need to make the extension so when the auth exception triggers, it will redirect to homepage
+In Web proj, create a folder called Extensions, inside create a class called AuthExceptionRedirection
+this class will inherit from IExceptionFilter
+Implement the filter in program.cs of web proj: inside AddcontrollersWithViews










