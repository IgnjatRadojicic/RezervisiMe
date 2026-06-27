
using System;
using System.Threading.Tasks;
using System.Web.Http;
using RezervisiMe.RezervisiMe.API.Auth;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Controllers
{
    [RoutePrefix("api/accommodations")]
    public class AccommodationsController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult List(
            string name = null, string city = null, string type = null,
            decimal? priceMin = null, decimal? priceMax = null,
            string sortBy = "postedAt", string sortDir = "desc",
            bool? isAvailable = null)
        {
            var c = new AccommodationSearchCriteria
            {
                Name = name,
                City = city,
                PriceMin = priceMin,
                PriceMax = priceMax,
                SortBy = sortBy,
                SortDir = sortDir,
                IsAvailable = isAvailable
            };
            if (!string.IsNullOrWhiteSpace(type)
                && Enum.TryParse<AccommodationType>(type, true, out var t))
                c.Type = t;

            var current = Request.GetCurrentUser();
            if (current == null || current.Role != "Administrator")
                c.IsAvailable = true;

            return Ok(Composition.AccommodationService.Search(c));
        }

        [HttpGet, Route("{id:guid}")]
        public IHttpActionResult Get(Guid id)
        {
            return Composition.AccommodationService.GetById(id).ToHttpResult(this);
        }

        [HttpGet, Route("mine")]
        [AuthorizeRole("Domacin")]
        public IHttpActionResult Mine(
            string sortBy = "postedAt", string sortDir = "desc", bool? isAvailable = null)
        {
            var current = Request.GetCurrentUser();
            var c = new AccommodationSearchCriteria
            {
                HostId = current.UserId,
                SortBy = sortBy,
                SortDir = sortDir,
                IsAvailable = isAvailable
            };
            return Ok(Composition.AccommodationService.Search(c));
        }

        [HttpPost, Route("upload-image")]
        [AuthorizeRole("Domacin", "Administrator")]
        public async Task<IHttpActionResult> UploadImage()
        {
            var result = await ImageUploader.SaveFromRequest(Request);
            return result.ToHttpResult(this);
        }

        [HttpPost, Route("")]
        [AuthorizeRole("Domacin")]
        public IHttpActionResult Create([FromBody] CreateAccommodationRequest req)
        {
            var current = Request.GetCurrentUser();
            return Composition.AccommodationService.Create(current.UserId, req).ToHttpResult(this);
        }

        [HttpPut, Route("{id:guid}")]
        [AuthorizeRole("Domacin", "Administrator")]
        public IHttpActionResult Update(Guid id, [FromBody] UpdateAccommodationRequest req)
        {
            var current = Request.GetCurrentUser();
            var role = (UserRole)Enum.Parse(typeof(UserRole), current.Role);
            return Composition.AccommodationService
                .Update(id, req, current.UserId, role)
                .ToHttpResult(this);
        }

        [HttpDelete, Route("{id:guid}")]
        [AuthorizeRole("Domacin", "Administrator")]
        public IHttpActionResult Delete(Guid id)
        {
            var current = Request.GetCurrentUser();
            var role = (UserRole)Enum.Parse(typeof(UserRole), current.Role);
            return Composition.AccommodationService
                .Delete(id, current.UserId, role)
                .ToHttpResult(this);
        }
    }
}