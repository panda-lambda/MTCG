using System.Text.Json.Serialization;
using System.Text.Json;

namespace MTCG.Utilities
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions DefaultOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        public static readonly JsonSerializerOptions NullOptions = new ()
        {
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };



    }
}
