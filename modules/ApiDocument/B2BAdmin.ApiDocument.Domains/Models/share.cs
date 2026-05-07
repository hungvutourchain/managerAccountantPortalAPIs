using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    public class JsonNullableDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out double value))
            {
                return value;
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                if (double.IsInfinity(value.Value) || double.IsNaN(value.Value))
                {
                    writer.WriteStringValue(value.Value.ToString());
                }
                else
                {
                    writer.WriteNumberValue(value.Value);
                }
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
    public class JsonNullableStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out double value))
            {
                return value.ToString();
            }

            return string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}