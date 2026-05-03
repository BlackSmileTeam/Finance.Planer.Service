using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinancialPlanner.Api.Json;

/// <summary>
/// Serializes <see cref="DateOnly"/> as "yyyy-MM-dd" string for API consistency.
/// </summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            return DateOnly.ParseExact(s ?? "", Format);
        }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            int year = 0, month = 0, day = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                var prop = reader.GetString();
                reader.Read();
                if (prop == "year") year = reader.GetInt32();
                else if (prop == "month") month = reader.GetInt32();
                else if (prop == "day") day = reader.GetInt32();
            }
            return new DateOnly(year, month, day);
        }
        throw new JsonException($"Unexpected token for DateOnly: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
