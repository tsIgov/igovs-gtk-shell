using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Igs.TypedIds;

public class TypedIdJsonConverterFactory : JsonConverterFactory
{
	private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(TypedId));

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		=> Cache.GetOrAdd(typeToConvert, createConverter);

	private static JsonConverter createConverter(Type typeToConvert)
	{
		Type converterType = typeof(IdJsonConverter<>).MakeGenericType(typeToConvert);
		return (JsonConverter)Activator.CreateInstance(converterType)!;
	}
}