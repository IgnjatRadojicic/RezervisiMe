
using System;
using System.Web.Http;
using RezervisiMe.RezervisiMe.API.Auth;
using RezervisiMe.RezervisiMe.API.Infrastructure;
using RezervisiMe.RezervisiMe.API.Models;
using RezervisiMe.RezervisiMe.API.Models.Requests;

namespace RezervisiMe.RezervisiMe.API.Controllers
{
    [RoutePrefix("api/reservations")]
    public class ReservationsController : ApiController
    {
        [HttpGet, Route("mine")]
        [AuthorizeRole("Gost")]
        public IHttpActionResult Mine(string status = null)
        {
            var current = Request.GetCurrentUser();
            ReservationStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<ReservationStatus>(status, true, out var s))
                filter = s;
            return Ok(Composition.ReservationService.GetByGuest(current.UserId, filter));
        }

        [HttpGet, Route("")]
        [AuthorizeRole("Administrator")]
        public IHttpActionResult All(string status = null, Guid? accommodationId = null)
        {
            ReservationStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<ReservationStatus>(status, true, out var s))
                filter = s;
            return Ok(Composition.ReservationService.GetAll(filter, accommodationId));
        }

        [HttpPost, Route("")]
        [AuthorizeRole("Gost")]
        public IHttpActionResult Create([FromBody] CreateReservationRequest req)
        {
            var current = Request.GetCurrentUser();
            return Composition.ReservationService.Create(current.UserId, req).ToHttpResult(this);
        }

        [HttpPost, Route("{id:guid}/cancel")]
        [AuthorizeRole("Gost", "Administrator")]
        public IHttpActionResult Cancel(Guid id)
        {
            var current = Request.GetCurrentUser();
            var role = (UserRole)Enum.Parse(typeof(UserRole), current.Role);
            return Composition.ReservationService.Cancel(id, current.UserId, role).ToHttpResult(this);
        }

        [HttpPost, Route("{id:guid}/approve")]
        [AuthorizeRole("Administrator")]
        public IHttpActionResult Approve(Guid id)
        {
            return Composition.ReservationService.Approve(id).ToHttpResult(this);
        }

        [HttpPost, Route("{id:guid}/reject")]
        [AuthorizeRole("Administrator")]
        public IHttpActionResult Reject(Guid id)
        {
            return Composition.ReservationService.Reject(id).ToHttpResult(this);
        }
    }
}