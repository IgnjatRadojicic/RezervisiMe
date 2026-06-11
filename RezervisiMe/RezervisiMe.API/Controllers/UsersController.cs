
using System;
using System.Globalization;
using System.Web.Http;
using RezervisiMe.RezervisiMe.API.Auth;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Controllers
{
    [RoutePrefix("api/users")]
    [AuthorizeRole("Administrator")]  
    public class UsersController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Search(
            string firstName = null, string lastName = null,
            string dobFrom = null, string dobTo = null,
            string role = null,
            string sortBy = "name", string sortDir = "asc")
        {
            var c = new UserSearchCriteria
            {
                FirstName = firstName,
                LastName = lastName,
                SortBy = sortBy,
                SortDir = sortDir
            };

            if (!string.IsNullOrWhiteSpace(dobFrom)
                && DateTime.TryParseExact(dobFrom, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                c.DateOfBirthFrom = from;

            if (!string.IsNullOrWhiteSpace(dobTo)
                && DateTime.TryParseExact(dobTo, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                c.DateOfBirthTo = to;

            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var r))
                c.Role = r;

            return Ok(Composition.UserService.Search(c));
        }

        [HttpGet, Route("{id:guid}")]
        public IHttpActionResult GetById(Guid id)
        {
            return Composition.UserService.GetById(id).ToHttpResult(this);
        }

        [HttpPost, Route("host")]
        public IHttpActionResult CreateHost([FromBody] RegisterRequest req)
        {
            if (req != null) req.Role = UserRole.Domacin;
            return Composition.UserService.Register(req, createdByAdmin: true).ToHttpResult(this);
        }

        [HttpPut, Route("{id:guid}")]
        public IHttpActionResult Update(Guid id, [FromBody] UpdateProfileRequest req)
        {
            return Composition.UserService.UpdateProfile(id, req).ToHttpResult(this);
        }

        [HttpDelete, Route("{id:guid}")]
        public IHttpActionResult Delete(Guid id)
        {
            return Composition.UserService.DeleteUser(id).ToHttpResult(this);
        }
    }
}