using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RezervisiMe.RezervisiMe.API.Auth
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles ?? new string[0];
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            if (auth == null || auth.Scheme != "Bearer") return false;

            var entry = TokenStore.Validate(auth.Parameter);
            if (entry == null) return false;

            actionContext.Request.Properties["CurrentUser"] = entry;

            if (_roles.Length == 0) return true;
            return _roles.Contains(entry.Role);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                new { error = "Nije autorizovan" });
        }
    }

    public static class RequestExtensions
    {
        public static TokenStore.TokenEntry GetCurrentUser(this HttpRequestMessage req)
        {
            if (req == null) return null;
            if (req.Properties.TryGetValue("CurrentUser", out var obj))
                return obj as TokenStore.TokenEntry;
            return null;
        }
    }
}