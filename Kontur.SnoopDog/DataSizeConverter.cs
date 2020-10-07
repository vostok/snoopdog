using System;
using Kontur.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontur.SnoopDog
{
    internal class DataSizeConverter : JsonConverter<DataSize>
    {
        public override void WriteJson(JsonWriter writer, DataSize value, JsonSerializer serializer)
        {
            var o = new JObject();
            o.AddFirst(new JProperty("Value", value.Bytes));
            o.AddFirst(new JProperty("Unit", "Bytes"));
            
            o.WriteTo(writer);
        }

        public override DataSize ReadJson(JsonReader reader, Type objectType, DataSize existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}