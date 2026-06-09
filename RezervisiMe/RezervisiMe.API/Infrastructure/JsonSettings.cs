// Infrastructure/JsonSettings.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{

    public static class JsonSettings
    {
        public static JsonSerializerSettings ForFileStore
        {
            get
            {
                var s = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };
                s.Converters.Add(new StringEnumConverter());   // enum -> string u JSON-u
                return s;
            }
        }

        public static JsonSerializerSettings ForApi
        {
            get
            {
                var s = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                s.Converters.Add(new StringEnumConverter());
                return s;
            }
        }
    }
}