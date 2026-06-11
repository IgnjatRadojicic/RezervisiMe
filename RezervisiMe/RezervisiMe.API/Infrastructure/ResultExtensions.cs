using System.Net;
using System.Web.Http;
using System.Web.Http.Results;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public static class ResultExtensions
    {
        public static IHttpActionResult ToHttpResult<T>(this Result<T> result, ApiController controller)
        {
            if (result.IsSuccess)
                return new OkNegotiatedContentResult<T>(result.Value, controller);
            return MapError(result.Error, controller);
        }

        public static IHttpActionResult ToHttpResult(this Result result, ApiController controller)
        {
            if (result.IsSuccess)
                return new OkResult(controller);
            return MapError(result.Error, controller);
        }

        private static IHttpActionResult MapError(Error error, ApiController controller)
        {
            var body = new { error = error.Description, code = error.Code };
            HttpStatusCode status;
            switch (error.Code)
            {
                case "NotFound": status = HttpStatusCode.NotFound; break;
                case "Validation": status = HttpStatusCode.BadRequest; break;
                case "Conflict": status = HttpStatusCode.Conflict; break;
                case "Forbidden": status = HttpStatusCode.Forbidden; break;
                case "Unauthorized": status = HttpStatusCode.Unauthorized; break;
                default: status = HttpStatusCode.InternalServerError; break;
            }
            return new NegotiatedContentResult<object>(status, body, controller);
        }
    }
}