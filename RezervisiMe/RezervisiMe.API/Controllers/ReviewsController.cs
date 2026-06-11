
using System;
using System.Web.Http;
using RezervisiMe.RezervisiMe.API.Auth;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Controllers
{
    [RoutePrefix("api/reviews")]
    public class ReviewsController : ApiController
    {
        [HttpGet, Route("for-accommodation/{accommodationId:guid}")]
        public IHttpActionResult ForAccommodation(Guid accommodationId)
        {
            var current = Request.GetCurrentUser();
            UserRole? role = null;
            if (current != null && Enum.TryParse<UserRole>(current.Role, out var r))
                role = r;
            return Ok(Composition.ReviewService.GetForAccommodation(accommodationId, role));
        }

        [HttpGet, Route("")]
        [AuthorizeRole("Administrator")]
        public IHttpActionResult All(string status = null)
        {
            ReviewStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<ReviewStatus>(status, true, out var s))
                filter = s;
            return Ok(Composition.ReviewService.GetAll(filter));
        }

        [HttpGet, Route("mine")]
        [AuthorizeRole("Gost")]
        public IHttpActionResult Mine()
        {
            var current = Request.GetCurrentUser();
            return Ok(Composition.ReviewService.GetByReviewer(current.UserId));
        }

        [HttpPost, Route("")]
        [AuthorizeRole("Gost")]
        public IHttpActionResult Create([FromBody] CreateReviewRequest req)
        {
            var current = Request.GetCurrentUser();
            return Composition.ReviewService.Create(current.UserId, req).ToHttpResult(this);
        }

        [HttpPut, Route("{id:guid}")]
        [AuthorizeRole("Gost")]
        public IHttpActionResult Update(Guid id, [FromBody] UpdateReviewRequest req)
        {
            var current = Request.GetCurrentUser();
            return Composition.ReviewService.Update(id, req, current.UserId).ToHttpResult(this);
        }

        [HttpDelete, Route("{id:guid}")]
        [AuthorizeRole("Gost", "Administrator")]
        public IHttpActionResult Delete(Guid id)
        {
            var current = Request.GetCurrentUser();
            var role = (UserRole)Enum.Parse(typeof(UserRole), current.Role);
            return Composition.ReviewService.Delete(id, current.UserId, role).ToHttpResult(this);
        }

        [HttpPost, Route("{id:guid}/approve")]
        [AuthorizeRole("Administrator")]
        public IHttpActionResult Approve(Guid id)
        {
            return Composition.ReviewService.Approve(id).ToHttpResult(this);
        }

        [HttpPost, Route("{id:guid}/reject")]
        [AuthorizeRole("Administrator")]
        public IHttpActionResult Reject(Guid id)
        {
            return Composition.ReviewService.Reject(id).ToHttpResult(this);
        }
    }
}