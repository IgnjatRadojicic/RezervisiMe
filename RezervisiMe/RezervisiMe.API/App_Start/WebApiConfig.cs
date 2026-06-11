using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace RezervisiMe.RezervisiMe.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.Converters.Add(new StringEnumConverter());
            json.SerializerSettings.Formatting = Formatting.Indented;
            json.UseDataContractJsonSerializer = false;
        }
    }
}