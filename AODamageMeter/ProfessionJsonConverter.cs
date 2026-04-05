using Newtonsoft.Json;
using System;
using System.Linq;

namespace AODamageMeter
{
    public class ProfessionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Profession);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string name = (string)reader.Value;
            return Profession.All.SingleOrDefault(p => p.Name == name) ?? Profession.Unknown;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => writer.WriteValue(((Profession)value)?.Name);
    }
}
