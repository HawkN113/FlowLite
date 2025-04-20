using System.Text.Json;
using System.Text.Json.Serialization;
namespace FlowLite.Core.Converters;

internal class TupleConverter<TState, TEntity> : JsonConverter<(TState? State, TEntity? Entity)>
{
    private const string StatePropertyName = "State";
    private const string EntityPropertyName = "Entity";

    public override (TState? State, TEntity? Entity) Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var state = JsonSerializer.Deserialize<TState>(root.GetProperty(StatePropertyName).GetRawText());
        var entity = JsonSerializer.Deserialize<TEntity>(root.GetProperty(EntityPropertyName).GetRawText());
        return (state, entity);
    }

    public override void Write(Utf8JsonWriter writer, (TState? State, TEntity? Entity) value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(StatePropertyName);
        JsonSerializer.Serialize(writer, value.State, options);

        writer.WritePropertyName(EntityPropertyName);
        JsonSerializer.Serialize(writer, value.Entity, options);

        writer.WriteEndObject();
    }
}