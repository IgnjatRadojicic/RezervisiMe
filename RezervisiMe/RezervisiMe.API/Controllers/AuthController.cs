using System.Web.Http;
using RezervisiMe.RezervisiMe.API.Auth;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models.Dto;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public UserDto User { get; set; }
        }

        [HttpPost, Route("register")]
        public IHttpActionResult Register([FromBody] RegisterRequest req)
        {
            return Composition.UserService.Register(req, createdByAdmin: false).ToHttpResult(this);
        }

        [HttpPost, Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest req)
        {
            var result = Composition.UserService.Login(req?.Username, req?.Password);
            if (result.IsFailure) return result.ToHttpResult(this);

            var token = TokenStore.Issue(
                result.Value.Id,
                result.Value.UserName,
                result.Value.Role.ToString());

            return Ok(new LoginResponse { Token = token, User = result.Value });
        }

        [HttpPost, Route("logout")]
        [AuthorizeRole]
        public IHttpActionResult Logout()
        {
            var auth = Request.Headers.Authorization;
            if (auth != null) TokenStore.Revoke(auth.Parameter);
            return Ok();
        }

        [HttpGet, Route("me")]
        [AuthorizeRole]
        public IHttpActionResult Me()
        {
            var current = Request.GetCurrentUser();
            return Composition.UserService.GetById(current.UserId).ToHttpResult(this);
        }

        [HttpPut, Route("me")]
        [AuthorizeRole]
        public IHttpActionResult UpdateMe([FromBody] UpdateProfileRequest req)
        {
            var current = Request.GetCurrentUser();
            return Composition.UserService.UpdateProfile(current.UserId, req).ToHttpResult(this);
        }
    }
}