using GateEntryExit.Domain;
using Newtonsoft.Json;

namespace GateEntryExit.JsonConverters
{

    //✔ Deserialization
    //If JSON token is {, deserialize as a Gate.
    //If JSON is something else (e.g., null, "string", number), return null.

    // How to use this converter? In any property like below
    // [JsonConverter(typeof(Gate))]
    // [JsonProperty("gate")]
    // public Gate gate { get; set; }

    public class GateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Gate);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Gate gate = null;
            if(reader.TokenType == JsonToken.StartObject)
            {
                gate = serializer.Deserialize<Gate>(reader);
            }
            return gate;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
