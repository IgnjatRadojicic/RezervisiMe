using RezervisiMe.RezervisiMe.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace RezervisiMe
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            var test = Composition.Users.Add(new RezervisiMe.API.Models.User
            {
                UserName = "test-" + System.Guid.NewGuid().ToString("N").Substring(0, 6),
                PasswordHash = "placeholder",
                FirstName = "Test",
                LastName = "Korisnik",
                Email = "test@test.rs",
                DateOfBirth = new System.DateTime(1995, 1, 15),
                Gender = RezervisiMe.API.Models.Gender.Muski,
                Role = RezervisiMe.API.Models.UserRole.Gost
            });
            System.Diagnostics.Debug.WriteLine($"Dodat user sa Id={test.Id}");

            var all = Composition.Users.GetAll();
            System.Diagnostics.Debug.WriteLine($"Ukupno usera: {System.Linq.Enumerable.Count(all)}");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}