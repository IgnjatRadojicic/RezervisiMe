using System;
using System.Globalization;
using Newtonsoft.Json;

namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public class CustomDateConverter : JsonConverter<DateTime>
    {
        private const string Format = "dd/MM/yyyy";

        public override DateTime ReadJson(JsonReader reader, Type objectType,
            DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return default;
            return DateTime.ParseExact(reader.Value.ToString(), Format, CultureInfo.InvariantCulture);
        }
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}