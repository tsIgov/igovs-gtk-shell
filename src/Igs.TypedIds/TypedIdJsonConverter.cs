using System.Text.Json;
using System.Text.Json.Serialization;

namespace Igs.TypedIds;

public class IdJsonConverter<T> : JsonConverter<T>
	where T : TypedId
{
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		string value;
		if (reader.TokenType == JsonTokenType.Number)
			value = JsonSerializer.Deserialize<int>(ref reader, options)!.ToString();
		else if (reader.TokenType == JsonTokenType.String)
			value = JsonSerializer.Deserialize<string>(ref reader, options)!;
		else
			throw new ArgumentException("Can't infer strongly typed id value.");

		T result = (Activator.CreateInstance(typeof(T), value) as T)!;
		return result;
	}

	public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
	{
		if (value is null)
			writer.WriteNullValue();
		else
			JsonSerializer.Serialize(writer, value.Value, options);
	}
}