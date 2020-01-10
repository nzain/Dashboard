using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nZain.Dashboard.Services
{
    public static class HttpContentExtensions
    {
        // default System.Text.Json deserializer is very picky and only accepts one specific format... O_o
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Converters = {new DateTimeConverterUsingDateTimeParse()}
        };

        public static async Task<T> ReadAsAsync<T>(this HttpContent content) 
        {
            using (Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, Options).ConfigureAwait(false);
            }
        }
    }

    public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTimeOffset));
            return DateTimeOffset.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}