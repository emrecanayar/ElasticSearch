using Nest;
using Newtonsoft.Json;

namespace ElasticSearch.AdvancedAPI.Extensions
{
    public class IndexNameJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IndexName);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException("IndexName should be a string.");
            }

            string indexName = (string)reader.Value;
            return indexName;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var indexName = (IndexName)value;
            writer.WriteValue(indexName.Name);
        }
    }
}
