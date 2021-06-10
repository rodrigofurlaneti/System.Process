using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace System.Process.Domain.ValueObjects
{
    public class Credentials
    {
        [JsonProperty("__type")]
        [JsonPropertyName("__type")]
        public string type { get; set; }
        public int EntityId { get; set; }
        public int StoreId { get; set; }
        public string StoreKey { get; set; }
    }
}
